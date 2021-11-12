using AngleSharp.Common;
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
using MigrationToolWebAPI.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MigrationToolWebAPI
{
    public class Startup
    {
        private static readonly string policy = "DestJsonPolicy";
        private static string destinationpathJson = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"DestinationJSON\");
        private static string FinalPathsConfig = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"\Config\") + "FinalPaths.json";
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MigrationToolWebAPI", Version = "v1" });
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
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MigrationToolWebAPI v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(policy);

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                app.UseEndpoints(endpoints => {
                    endpoints.MapGet("/", async context => {
                        try
                        {

                            await MainTool.toolProcessAsync();

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


                              string url = context.Request.Query.First().Value.ToString().Trim().Replace("file:///", "");
                              string destJson = "";
                              bool found = false;

                              StreamReader read = new StreamReader(FinalPathsConfig);
                              string jsonData = read.ReadToEnd();
                              var deserializeObject = (JArray)JsonConvert.DeserializeObject(jsonData);
                              foreach (var element in deserializeObject.Children())
                                  {
                                    //element.Children()[2]=destinationHTML key in FinalPaths.json

                                    foreach (var dest in element.Children())
                                      {
                                          if (dest.First().ToString().Contains(".json"))
                                          {
                                              destJson = dest.First().ToString();
                                          }
                                          if (dest.First().ToString().Trim().Contains(".html") && Path.GetFileName(dest.First().ToString().Trim()).Equals(Path.GetFileName(url)))
                                          {
                                              found = true;
                                              break;
                                          }
                                          else if (dest.First().ToString().Trim().Contains(".html") && !dest.First().ToString().Trim().Equals(url))
                                          {
                                              destJson = "";
                                          }
                                      };
                                      if (found)
                                      {
                                          break;
                                      }


                                  }
                                  
                                  string json = JsonConvert.SerializeObject(destJson);
                                  await context.Response.WriteAsync(json);
                                  read.Close();
                              
                              
                              
                          }
                          catch (Exception ex)
                          {
                              throw new Exception(ex.Message);
                          }
                      });

                    endpoints.MapGet("/destjson", async context =>
                    {
                        try
                        {
                            string destjson = context.Request.Query.First().Value.ToString().Trim();
                            StreamReader read = new StreamReader(destjson);
                            string jsonData = read.ReadToEnd();
                            string json = JsonConvert.SerializeObject(jsonData);
                            await context.Response.WriteAsync(json);
                            read.Close();

                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    });
                });
            });
        }
    }
}
