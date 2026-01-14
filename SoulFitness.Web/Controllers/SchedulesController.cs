using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Globalization;
using SoulFitness.DataObjects.Data;
using Microsoft.AspNetCore.Identity;
using SoulFitness.DataObjects.UserManagment.Identity;
using System.Security.Claims;
using SoulFitness.Abstractions;
using Microsoft.AspNetCore.Mvc.Rendering;
using SoulFitness.DataObjects;

namespace SoulFitness.Web.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SchedulesController : ControllerBase
    {
        private readonly ApplicationDbContext applicationDbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public SchedulesController(ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager)
        {
            this.applicationDbContext = applicationDbContext;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Schedules>>> Index()
        {
            var schedules = await applicationDbContext.Schedule.Where(e => e.status == Status.Active).ToListAsync();
            return Ok(schedules);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Schedules>> Details(int id)
        {
            var schedule = await applicationDbContext.Schedule.FirstOrDefaultAsync(m => m.Id == id);
            if (schedule == null) return NotFound();
            return Ok(schedule);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Schedules sched)
        {

            if (await applicationDbContext.Schedule.AnyAsync(i => i.TimeInterval == sched.TimeInterval))
            {
                return BadRequest("Schedule interval already exists.");
            }

            var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var loggedInUser = await userManager.FindByIdAsync(loggedInUserId);

            sched.CreatedAt = DateTime.Now;
            sched.status = Status.Active;
            sched.CreatedBy = loggedInUser?.Email ?? "System";
            applicationDbContext.Schedule.Add(sched);
            await applicationDbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(Details), new { id = sched.Id }, sched);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] Schedules sched)
        {
            if (id != sched.Id) return BadRequest("ID mismatch.");

            try
                {
                    var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var loggedInUser = await userManager.FindByIdAsync(loggedInUserId);

                    sched.UpdatedAt = DateTime.Now;
                    sched.UpdatedBy = loggedInUser?.Email;
                    sched.status = Status.Active;
                    applicationDbContext.Update(sched);
                    await applicationDbContext.SaveChangesAsync();
                    return Ok(new { Message = "Schedule updated successfully." });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!applicationDbContext.Schedule.Any(e => e.Id == id)) return NotFound();
                    throw;
                }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var sched = await applicationDbContext.Schedule.FindAsync(id);
            if (sched == null) return NotFound();

            sched.status = Status.Inactive;
            await applicationDbContext.SaveChangesAsync();
            return Ok(new { Message = "Schedule deactivated." });
        }
    }
}
