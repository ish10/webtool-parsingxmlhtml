using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WebScrappingWebAPI
{
    public class Startup
    {
        private static readonly string policy = "AttributeListPolicy";
        private static string jsonPath = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"JSON\");
        public IConfiguration Configuration { get; }


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebScrappingWebAPI", Version = "v1" });
            });
            services.AddCors(options =>
            {
                options.AddPolicy(policy, builder => {
                    builder.AllowAnyOrigin();
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebScrappingWebAPI v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors(policy);
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    try
                    {

                        await SourceXML.toolProcessAsync();

                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                });

                endpoints.MapGet("/json", async context =>
                {
                    try
                    {

                        StreamReader reader = new StreamReader(jsonPath + "WebsiteConfig.json");
                        string jsonString = reader.ReadToEnd();
                        await context.Response.WriteAsync(jsonString);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                });
            });
        }
    }
}
