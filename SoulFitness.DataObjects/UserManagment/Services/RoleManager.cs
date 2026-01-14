using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SoulFitness.Abstractions;
using SoulFitness.DataObjects.UserManagment.Identity;
using System;
using System.Threading.Tasks;

namespace SoulFitness.DataObjects.UserManagment.Services
{
    public class RoleManager : IRoleManager
    {
        readonly IServiceProvider serviceProvider;
        readonly RoleManager<ApplicationRole> roleManager;

        public RoleManager(IServiceProvider _serviceProvider)
        {
            serviceProvider = _serviceProvider;

            var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            roleManager = serviceScope.ServiceProvider.GetService<RoleManager<ApplicationRole>>();
        }

        public async Task<bool> CreateRole(string roleName, string description)
        {
            var idResult = await roleManager.CreateAsync(new ApplicationRole(roleName, description));
            return idResult.Succeeded;
        }

        public async Task<bool> RoleExists(string roleName)
        {
            return await roleManager.RoleExistsAsync(roleName);
        }
    }
}
