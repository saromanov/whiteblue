using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;

namespace Users
{
    class Program
    {
        static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

         public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder()
                .UseApplicationInsights()
                .UseSerilog()
                .UseHealthChecks("/healthz")
                .UseStartup<Startup>()
                .Build();
        }
    }
}
