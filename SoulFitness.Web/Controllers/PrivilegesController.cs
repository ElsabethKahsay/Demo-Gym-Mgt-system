using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoulFitness.DataObjects.Data;
using SoulFitness.DataObjects.UserManagment.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoulFitness.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrivilegesController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public PrivilegesController(ApplicationDbContext db)
        {
            context = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationPrivilege>>> GetPrivileges()
        {
            var privileges = await context.ApplicationPrivileges.OrderBy(p => p.Action).ToListAsync();
            return Ok(privileges);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] ApplicationPrivilege model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await context.ApplicationPrivileges.FirstOrDefaultAsync(i => i.Action == model.Action);
            if (existing != null)
            {
                return Conflict("Privilege already exists!");
            }

            model.Id = Guid.NewGuid().ToString();
            context.ApplicationPrivileges.Add(model);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPrivileges), new { id = model.Id }, model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] ApplicationPrivilege model)
        {
            if (id != model.Id) return BadRequest("ID mismatch.");

            if (ModelState.IsValid)
            {
                context.Entry(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return Ok(new { Message = "Privilege updated successfully." });
            }
            return BadRequest(ModelState);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var privilege = await context.ApplicationPrivileges.FindAsync(id);
            if (privilege == null) return NotFound();

            var rolePrivileges = context.ApplicationRolePrivileges.Where(rp => rp.PrivilegeId == privilege.Id);
            privilege.status = DataObjects.Status.Inactive;
            foreach (var rp in rolePrivileges)
            {
                rp.status = DataObjects.Status.Inactive;
            }

            await context.SaveChangesAsync();
            return Ok(new { Message = "Privilege deactivated." });
        }
    }
}
