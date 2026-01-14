using Hangfire.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SoulFitness.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utility = SoulFitness.Utilities.Utility;

namespace SoulFitness.web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Utility.WriteLog("Starting Service");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Utility.WriteLog(ex?.Message);
            }

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();                  // If using controllers

            // Required for generating OpenAPI metadata (especially useful for Minimal APIs)
            builder.Services.AddEndpointsApiExplorer();

            // Add Swagger generator with basic configuration
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ADD Performance API",
                    Version = "v1",
                    Description = "API for managing REV_USD, Corporate Sales, ADD_CK, Destinations, etc."
                });
            });


            var app = builder.Build();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();           // Serves /swagger/v1/swagger.json
                app.UseSwaggerUI(c =>       // Serves the nice UI at /swagger
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ADD Performance API v1");
                    c.RoutePrefix = "swagger";  // Optional: access at /swagger (default)
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();           // or your endpoint mappings

            app.Run();
        }
public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
