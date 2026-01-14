using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoulFitness.DataObjects.Data;
using SoulFitness.Abstractions;
using SoulFitness.DataObjects.UserManagment;
using SoulFitness.DataObjects.UserManagment.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace SoulFitness.DataObjects.Data
{
    public class DbInitializer : IDbInitializer
    {
        private readonly IServiceProvider serviceProvider;
        public IConfiguration Configuration { get; }

        public DbInitializer(IServiceProvider _serviceProvider, IConfiguration _configuration)
        {
            serviceProvider = _serviceProvider;
            Configuration = _configuration;
        }

        public async void Initialize()
        {
            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                context.Database.EnsureCreated();

                if (!context.Roles.Any())
                {
                    var _roleManager = serviceScope.ServiceProvider.GetService<RoleManager<ApplicationRole>>();

                    foreach (UserRole role in (UserRole[])Enum.GetValues(typeof(UserRole)))
                    {
                        var description = role.ToString();
                        if (!await _roleManager.RoleExistsAsync(role.ToString()))
                        {
                            await _roleManager.CreateAsync(new ApplicationRole(role.ToString(), description, role));
                        }
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}