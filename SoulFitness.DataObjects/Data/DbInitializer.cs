
using Microsoft.EntityFrameworkCore.Internal;
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

        //Seed, Creat admin role and users, and assign privileges
        public async void Initialize()
        {
            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                //create database schema if none exists
                var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                context.Database.EnsureCreated();

                if (context.Users.Any())
                {
                    //If there is already an Administrator role, abort
                    var _roleManager = serviceScope.ServiceProvider.GetService<RoleManager<ApplicationRole>>();

                    foreach (UserRole role in (UserRole[])Enum.GetValues(typeof(UserRole)))
                    {
                        var description = role.ToString();
                        // if (await _roleManager.RoleExistsAsync(role.ToString()))
                        if (!context.Users.Any())
                                                    {
                            //Create Role
                            var result = await _roleManager.CreateAsync(new ApplicationRole(role.ToString(), description, role));

                            if (!result.Succeeded)
                                return;
                        }
                        context.SaveChanges();
                    }

                    var _userManager = serviceScope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
                    
                    //Admin User
                    var usr = new ApplicationUser
                    {
                        UserName = Configuration.GetSection("UserSettings")["DefaultUsername"],// "22962",
                        Email = Configuration.GetSection("UserSettings")["DefaultEmail"],//"GebreegziabherG@ethiopianairlines.com",
                        PhoneNumber = Configuration.GetSection("UserSettings")["DefaultPhone"],//"+251115174547",
                        CreatedAt = DateTime.Now,
                        CreatedBy = "Initial",
                    };
                    var success = await _userManager.CreateAsync(usr, Configuration.GetSection("UserSettings")["DefaultPassword"]);
                    if (success.Succeeded)
                        await _userManager.AddToRoleAsync(await _userManager.FindByNameAsync(usr.UserName), (UserRole.Admin).ToString());
                }
            }
        }
    }
}