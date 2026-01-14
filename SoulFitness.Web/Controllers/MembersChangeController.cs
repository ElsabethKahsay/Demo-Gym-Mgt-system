using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using SoulFitness.DataObjects.Data;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using SoulFitness.DataObjects.UserManagment.Identity;
using System.Data;
using SoulFitness.Web.Controllers;
using System.Text;
using Microsoft.AspNetCore.Http;
using SoulFitness.DataObjects;

namespace SoulFitness.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembersChangeController : ControllerBase
    {
        private readonly ApplicationDbContext applicationDbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public MembersChangeController(ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager)
        {
            this.applicationDbContext = applicationDbContext;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> Index([FromQuery] string searchingField = null)
        {
            if (!string.IsNullOrEmpty(searchingField))
            {
                var em = await applicationDbContext.Employees
                    .Include(e => e.Schedule)
                    .FirstOrDefaultAsync(e => e.EmployeeID == searchingField);
                
                if (em != null) return Ok(new List<Employee> { em });
                return NotFound("No employee with this ID found.");
            }

            var inactiveEmployees = await applicationDbContext.Employees
                .Include(e => e.Schedule)
                .Where(e => e.Status == Status.Inactive)
                .OrderByDescending(e => e.UpdatedAt)
                .ToListAsync();

            return Ok(inactiveEmployees);
        }

        [HttpPost("import-inactive")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ImportInactiveMembers(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Please upload a file.");

            string fileExt = Path.GetExtension(file.FileName);
            if (!fileExt.Equals(".csv", StringComparison.OrdinalIgnoreCase) && !fileExt.Equals(".txt", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Invalid file format. Please use .csv or .txt.");
            }

            var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var loggedInUser = await userManager.FindByIdAsync(loggedInUserId);

            var updatedMembers = new List<Employee>();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(line)) continue;

                    var values = line.Split(',');
                    if (values.Length > 7)
                    {
                        var tempID = values[7].Trim();
                        var member = await applicationDbContext.Employees.FirstOrDefaultAsync(i => i.EmployeeID == tempID);
                        if (member != null)
                        {
                            member.Status = Status.Inactive;
                            member.UpdatedAt = DateTime.Now;
                            member.UpdatedBy = loggedInUser?.Email ?? "System";
                            updatedMembers.Add(member);
                        }
                    }
                }
            }

            if (updatedMembers.Any())
            {
                applicationDbContext.Employees.UpdateRange(updatedMembers);
                await applicationDbContext.SaveChangesAsync();
                return Ok(new { Message = $"{updatedMembers.Count} members marked as inactive.", Data = updatedMembers });
            }

            return BadRequest("No matching members found in the file.");
        }

        [HttpGet("csv/inactive")]
        public async Task<ActionResult> ToCSVInactive([FromQuery] string from, [FromQuery] string to)
        {
            if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
            {
                return BadRequest("Invalid date format.");
            }

            var emp = await applicationDbContext.Employees.Include(e => e.Schedule)
                .Where(e => e.UpdatedAt >= fromDate && e.UpdatedAt < toDate && e.Status == Status.Inactive)
                .ToListAsync();

            return ConvertToCSV(emp, $"InactiveMembers_{from}_to_{to}.csv");
        }

        [HttpGet("csv/active")]
        public async Task<ActionResult> ToCSVActive([FromQuery] string from, [FromQuery] string to)
        {
            if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
            {
                return BadRequest("Invalid date format.");
            }

            var emp = await applicationDbContext.Employees.Include(e => e.Schedule)
                .Where(e => e.CreatedAt >= fromDate && e.CreatedAt < toDate && e.Status == Status.Active)
                .ToListAsync();

            return ConvertToCSV(emp, $"ActiveMembers_{from}_to_{to}.csv");
        }

        private ActionResult ConvertToCSV(List<Employee> empList, string fileName)
        {
            var builder = new StringBuilder();
            builder.AppendLine("ID, Name, Gender, DateTime, Status");
            foreach (var item in empList)
            {
                var date = item.Status == Status.Inactive ? item.UpdatedAt : item.CreatedAt;
                builder.AppendLine($"{item.EmployeeID}, {item.FullName}, {item.Gender}, {date}, {item.Status}");
            }
            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", fileName);
        }
    }
}
