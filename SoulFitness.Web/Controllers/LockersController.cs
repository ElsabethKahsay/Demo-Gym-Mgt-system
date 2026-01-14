using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoulFitness.DataObjects;
using SoulFitness.DataObjects.Data;
using SoulFitness.DataObjects.UserManagment.Identity;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SoulFitness.Web.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class LockersController : ControllerBase
    {
        private readonly ApplicationDbContext applicationDbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public LockersController(ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager)
        {
            this.applicationDbContext = applicationDbContext;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Locker>>> GetLockers()
        {
            return Ok(await applicationDbContext.Locker.ToListAsync());
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create([FromBody] Locker locker)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await applicationDbContext.Locker.AnyAsync(e => e.LockerNumber == locker.LockerNumber && e.Floor == locker.Floor && e.Gender == locker.Gender))
            {
                return BadRequest("Redundant locker found.");
            }

            var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var loggedInUser = await userManager.FindByIdAsync(loggedInUserId);

            locker.IsAssigned = false;
            locker.CreatedAt = DateTime.Now;
            locker.CreatedBy = loggedInUser?.Email ?? "System";
            applicationDbContext.Locker.Add(locker);
            await applicationDbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetLockers), new { id = locker.Id }, locker);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromBody] Locker locker, [FromQuery] int isAssigned)
        {
            if (id != locker.Id) return BadRequest("ID mismatch.");

            if (ModelState.IsValid)
            {
                try
                {
                    var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var loggedInUser = await userManager.FindByIdAsync(loggedInUserId);

                    locker.IsAssigned = isAssigned != 0;
                    locker.UpdatedAt = DateTime.Now;
                    locker.UpdatedBy = loggedInUser?.Email;

                    var activeAttendance = await applicationDbContext.GymAttendance
                        .Where(x => x.LockerNumber == locker.LockerNumber
                                    && x.FloorNumber == locker.Floor
                                    && x.Employees.Gender == locker.Gender
                                    && !x.IsReturned)
                        .FirstOrDefaultAsync();

                    if (activeAttendance != null && !locker.IsAssigned)
                    {
                        activeAttendance.IsReturned = true;
                        activeAttendance.ReturnDateAndTime = DateTime.Now;
                        applicationDbContext.GymAttendance.Update(activeAttendance);
                    }

                    applicationDbContext.Locker.Update(locker);
                    await applicationDbContext.SaveChangesAsync();
                    return Ok(new { Message = "Locker info successfully edited." });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!applicationDbContext.Locker.Any(e => e.Id == id)) return NotFound();
                    throw;
                }
            }
            return BadRequest(ModelState);
        }
    }
}
