
using Microsoft.AspNetCore.Hosting;
using MigrationTool.Pojo;
using Microsoft.Extensions.Hosting;

namespace MigrationTool
{
    class Program
    {

        
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder => {
                webBuilder.UseStartup<Startup>()
                          .UseIIS();
            });
        }
        

    }
}

