using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoulFitness.Abstractions;
using SoulFitness.DataObjects.Data;
using SoulFitness.DataObjects.UserManagment.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoulFitness.Web.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IRoleManager roleManager;
        private readonly IUserManager userManager;
        private readonly RoleManager<ApplicationRole> identityRoleManager;

        public RolesController(IRoleManager roleManager,
            ApplicationDbContext context,
            RoleManager<ApplicationRole> identityRoleManager,
            IUserManager userManager)
        {
            this.context = context;
            this.roleManager = roleManager;
            this.identityRoleManager = identityRoleManager;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationRole>>> Index()
        {
            var roles = await context.Roles.ToListAsync();
            return Ok(roles);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] RoleViewModel model, [FromQuery] string privilegeIds)
        {
            if (string.IsNullOrEmpty(privilegeIds)) return BadRequest("At least one privilege is required.");

            if (await roleManager.RoleExists(model.RoleName))
                return Conflict("Role name already used.");

            if (await roleManager.CreateRole(model.RoleName, model.Description))
            {
                var role = await context.Roles.FirstAsync(r => r.Name == model.RoleName);
                string[] privileges = privilegeIds.Split(',');
                foreach (var item in privileges)
                {
                    context.ApplicationRolePrivileges.Add(new ApplicationRolePrivilege { RoleId = role.Id, PrivilegeId = item });
                }
                await context.SaveChangesAsync();
                return CreatedAtAction(nameof(Index), new { name = model.RoleName }, model);
            }
            return StatusCode(500, "Error creating role.");
        }

        [HttpGet("{name}")]
        public async Task<ActionResult> GetRole(string name)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == name);
            if (role == null) return NotFound();

            var privileges = await context.ApplicationRolePrivileges
                .Where(x => x.RoleId == role.Id)
                .Select(x => x.PrivilegeId)
                .ToListAsync();

            return Ok(new
            {
                Role = role,
                Privileges = privileges
            });
        }

        [HttpPut("{oldName}")]
        public async Task<ActionResult> Edit(string oldName, [FromBody] RoleViewModel model, [FromQuery] string privilegeIds)
        {

            var role = await context.Roles.FirstOrDefaultAsync(x => x.Name == oldName);
            if (role == null) return NotFound("Role not found.");

            if (!string.IsNullOrEmpty(privilegeIds))
            {
                var currentPrivileges = context.ApplicationRolePrivileges.Where(x => x.RoleId == role.Id);
                context.ApplicationRolePrivileges.RemoveRange(currentPrivileges);

                string[] newPrivileges = privilegeIds.Split(',');
                foreach (var item in newPrivileges)
                {
                    context.ApplicationRolePrivileges.Add(new ApplicationRolePrivilege { RoleId = role.Id, PrivilegeId = item });
                }
            }

            role.Description = model.Description;
            role.Name = model.RoleName;
            context.Entry(role).State = EntityState.Modified;
            await context.SaveChangesAsync();

            return Ok(new { Message = "Role updated successfully." });
        }

        [HttpDelete("{name}")]
        public async Task<ActionResult> Delete(string name)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == name);
            if (role == null) return NotFound();

            var inUse = await context.UserRoles.AnyAsync(u => u.RoleId == role.Id);
            if (inUse) return BadRequest("Cannot delete role. It is assigned to users.");

            var privileges = context.ApplicationRolePrivileges.Where(p => p.RoleId == role.Id);
            foreach (var item in privileges)
                item.status = DataObjects.Status.Inactive;

            await userManager.DeleteRole(role.Id);
            return Ok(new { Message = "Role deleted successfully." });
        }
    }
}