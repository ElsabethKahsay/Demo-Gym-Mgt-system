using Microsoft.AspNetCore.Mvc;
using SoulFitness.DataObjects.Data;
using SoulFitness.DataObjects;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using SoulFitness.DataObjects.UserManagment.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;


namespace SoulFitness.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MisconductLogController : ControllerBase
    {
        private readonly ApplicationDbContext applicationDbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public MisconductLogController(ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager)
        {
            this.applicationDbContext = applicationDbContext;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MisconductLog>>> GetLogs()
        {
            return Ok(await applicationDbContext.MisconductLog.ToListAsync());
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create([FromBody] MisconductLog misconduct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var employee = await applicationDbContext.Employees.FirstOrDefaultAsync(e => e.EmployeeID == misconduct.EmployeeId);
            if (employee == null) return NotFound("Employee not found.");

            var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var loggedInUser = await userManager.FindByIdAsync(loggedInUserId);

            if (misconduct.MisconductStatus == MisconductStatus.Banned)
            {
                employee.Status = Status.Banned;
                applicationDbContext.Employees.Update(employee);
            }

            misconduct.CreatedAt = DateTime.Now;
            misconduct.CreatedBy = loggedInUser?.Email ?? "System";
            applicationDbContext.MisconductLog.Add(misconduct);
            await applicationDbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLogs), new { id = misconduct.Id }, misconduct);
        }

        [HttpGet("csv/warnings")]
        public async Task<ActionResult> ExportWarnings([FromQuery] string from, [FromQuery] string to)
        {
            if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
            {
                return BadRequest("Invalid date format. Use YYYY-MM-DD.");
            }

            var logs = await applicationDbContext.MisconductLog
                .Where(e => e.MisconductStatus != MisconductStatus.Banned && e.CreatedAt >= fromDate && e.CreatedAt < toDate)
                .ToListAsync();

            return ConvertToCSV(logs, $"Warnings_{from}_to_{to}.csv");
        }

        [HttpGet("csv/banned")]
        public async Task<ActionResult> ExportBanned([FromQuery] string from, [FromQuery] string to)
        {
            if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
            {
                return BadRequest("Invalid date format. Use YYYY-MM-DD.");
            }

            var logs = await applicationDbContext.MisconductLog
                .Where(e => e.MisconductStatus == MisconductStatus.Banned && e.CreatedAt >= fromDate && e.CreatedAt <= toDate)
                .ToListAsync();

            return ConvertToCSV(logs, $"Banned_{from}_to_{to}.csv");
        }

        private ActionResult ConvertToCSV(List<MisconductLog> logs, string fileName)
        {
            var builder = new StringBuilder();
            builder.AppendLine("ID, Name, Type, Status, Fine, DateTime, Reason, Admin");
            foreach (var item in logs)
            {
                builder.AppendLine($"{item.EmployeeId}, {item.FirstName} {item.LastName}, {item.MisconductType}, {item.MisconductStatus}, {item.FineAmount}, {item.CreatedAt}, {item.Notes}, {item.CreatedBy}");
            }
            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", fileName);
        }
    }
}