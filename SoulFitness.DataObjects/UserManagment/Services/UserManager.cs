using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SoulFitness.DataObjects.Data;
using SoulFitness.Abstractions;
using SoulFitness.DataObjects.UserManagment.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoulFitness.DataObjects.UserManagment.Services
{
    public class UserManager : IUserManager
    {
        readonly IServiceProvider serviceProvider;
        UserManager<ApplicationUser> userManager;
        ApplicationDbContext context;

        public UserManager(IServiceProvider _serviceProvider)
        {
            serviceProvider = _serviceProvider;
            var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            userManager = serviceScope.ServiceProvider.GetService<UserManager<ApplicationUser>>();

            context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
            context.Database.EnsureCreated();
        }

        public async Task<bool> AddUserToRole(string userId, string roleName)
        {
            var user = await userManager.FindByIdAsync(userId);
            var idResult = await userManager.AddToRoleAsync(user, roleName);
            return idResult.Succeeded;
        }

        public async Task ClearUserRoles(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var userRoles = context.UserRoles.Where(ur => ur.UserId == user.Id).ToList();

                var currentRoles = new List<IdentityUserRole<string>>();

                currentRoles.AddRange(userRoles);

                foreach (var role in currentRoles)
                {
                    string name = context.Roles.FirstOrDefault(x => x.Id == role.RoleId).Name;
                    await userManager.RemoveFromRoleAsync(user, name);
                }
            }
        }

        public async Task DeleteRole(string roleId)
        {
            var roleUsers = context.Users.Where(u => u.UserRoles.Any(r => r.RoleId == roleId));
            var role = context.Roles.Find(roleId);

            foreach (var user in roleUsers)
                await RemoveFromRole(user.Id, role.Name);

            context.Roles.Remove(role);
            context.SaveChanges();
        }

        public async Task RemoveFromRole(string userId, string roleName)
        {
            var user = await userManager.FindByIdAsync(userId);
            await userManager.RemoveFromRoleAsync(user, roleName);
        }
    }
}
