using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTool
{
    class Startup
    {
        private static string destinationpathJson = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToString().Replace(@"bin\Debug\net5.0", @"DestinationJSON\");

        public void ConfigurationServices(IServiceCollection services)
        { 
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            
            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapGet("/", async context => {
                    try { 
                    
                        await MainTool.toolProcessAsync();
                        
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                });

                endpoints.MapGet("/json", async context => {
                    try
                    {

                        StreamReader reader = new StreamReader(destinationpathJson+ "minecraft_destination.json");
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
