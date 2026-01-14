using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoulFitness.DataObjects.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoulFitness.DataObjects.UserManagment.Identity
{
    /// <summary>
    /// System user to be authorized
    /// </summary>
    public class SoulFitnessAdminUser
    {
        ApplicationDbContext context;
        private readonly IServiceProvider serviceProvider;
        public string Username { get; set; }

        private List<ApplicationUserRole> Roles = new List<ApplicationUserRole>();

        public SoulFitnessAdminUser(IServiceProvider _serviceProvider)
        {
            serviceProvider = _serviceProvider;
        }

        public SoulFitnessAdminUser(string _username, IServiceProvider serviceProvider)
        {
            var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
            context.Database.EnsureCreated();

            Username = _username;
            GetUserRolesPrivileges();
        }

        /// <summary>
        /// Gets user privileges using its role
        /// </summary>
        private void GetUserRolesPrivileges()
        {
            //get user
            var user = context.Users.Where(u => u.UserName == this.Username).FirstOrDefault();

            if (user != null)
            {
                var userRoles = context.UserRoles.Where(ur => ur.UserId == user.Id).ToList();

                foreach (var role in userRoles)
                {
                    this.Roles.Add(new ApplicationUserRole { RoleId = role.RoleId });
                }
            }
        }

        /// <summary>
        /// Checks whether a user has a given privilege
        /// </summary>
        /// <param name="requiredPrivilege">Privilege to be checked</param>
        /// <returns></returns>

        public bool HasPrivilege(string requiredPrivilege)
        {
            bool found = false;
            List<ApplicationRolePrivilege> rolePrivilegelist = context.ApplicationRolePrivileges.ToList();
            var privileges = context.ApplicationPrivileges.ToList();

            foreach (ApplicationUserRole userRole in this.Roles)
            {
                List<ApplicationRolePrivilege> rolePrivilege = rolePrivilegelist.Where(r => r.RoleId == userRole.RoleId).ToList();
                foreach (var privilege in rolePrivilege)
                {
                    found = privileges.Where(p => p.Action == requiredPrivilege && privilege.PrivilegeId == p.Id).ToList().Count > 0;
                    if (found)
                        break;
                }
                if (found)
                    break;
            }
            return found;
        }

        public bool IsUserLockedOut()
        {
            //get user
            var user = context.Users.Where(u => u.UserName == this.Username).FirstOrDefault();

            if (user != null)
                return user.LockedOut;
            return false;
        }
    }

    public class SoulFitnessAuthorizationFilter : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            await Task.Delay(1);

            if (context != null && context?.ActionDescriptor is ControllerActionDescriptor descriptor)
            {
                /*Create permission string based on the requested controller 
                 name and action name in the format 'controllername-action'*/
                string requiredPrivilege = String.Format("{0}-{1}", descriptor.ControllerName, descriptor.ActionName);

                /*Create an instance of our custom user authorisation object passing requesting
                  user's 'Windows Username' into constructor*/
                SoulFitnessAdminUser requestingUser = new SoulFitnessAdminUser(context.HttpContext.User.Identity.Name, context.HttpContext.RequestServices);

                if (requestingUser.Username == null || requestingUser.IsUserLockedOut())
                {
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary {
                                                { "action", "Login" },
                                                { "controller", "Users" }});
                }
                else
                {
                    //Check if the requesting user has the permission to run the controller's action
                    if (!requestingUser.HasPrivilege(requiredPrivilege))
                    {
                        /*User doesn't have the required permission and is not a SysAdmin, return our 
                          custom '401 Unauthorized' access error. Since we are setting 
                          filterContext.Result to contain an ActionResult page, the controller's 
                          action will not be run.

                          The custom '401 Unauthorized' access error will be returned to the 
                          browser in response to the initial request.*/
                        context.Result = new RedirectToRouteResult(new RouteValueDictionary {
                                                { "action", "Unauthorized" },
                                                { "controller", "Main" }});
                    }
                    /*If the user has the permission to run the controller's action, then 
                      filterContext.Result will be uninitialized and executing the controller's 
                      action is dependant on whether filterContext.Result is uninitialized.*/
                }
            }
        }
    }
}
