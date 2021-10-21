using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MigrationTool
{
    public class Startup
    {
        public void Configure(IApplicationBuilder builder)
        {
            builder.Run(appContext =>
            {
                return appContext.Response.WriteAsync("Hey, I'm from Web!");
            });
        }
    }
}
