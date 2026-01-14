using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SoulFitness.Utilities;
using System;

namespace SoulFitness.web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Utility.WriteLog("Starting Soul Fitness API");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Utility.WriteLog("Service failed to start: " + ex.Message);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
