using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SoulFitness.DataObjects;
using SoulFitness.DataObjects.Data;
using SoulFitness.DataObjects.UserManagment.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace SoulFitness.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ApplicationDbContext applicationDbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public ReportController(ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager)
        {
            this.applicationDbContext = applicationDbContext;
            this.userManager = userManager;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult> GetDashboardData()
        {
            var emp = await applicationDbContext.Employees.Include(i => i.Schedule).ToListAsync();

            // Gender Distribution
            var genderResult = emp.GroupBy(i => i.Gender)
                .Select(group => new DashboardInput
                {
                    Name = group.Key.ToString(),
                    Count = group.Count()
                }).ToList();

            // Schedule Distribution
            var timeIntervals = emp.Where(e => e.Schedule != null && e.Schedule.status == Status.Active)
                .Select(e => e.Schedule.TimeInterval)
                .GroupBy(t => t)
                .Select(group => new DashboardInput
                {
                    Name = group.Key,
                    Count = group.Count()
                }).ToList();

            // Top Consistent Users
            var currentYear = DateTime.Now.Year;
            var topAttendees = await applicationDbContext.GymAttendance
                .Where(x => x.DateAndTime.Year == currentYear)
                .GroupBy(x => x.EmpID)
                .Select(group => new
                {
                    EmpID = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var consistencyData = new List<DashboardInput>();
            foreach (var item in topAttendees)
            {
                var employee = await applicationDbContext.Employees.FindAsync(item.EmpID);
                if (employee != null)
                {
                    consistencyData.Add(new DashboardInput
                    {
                        Name = employee.FullName,
                        Count = item.Count
                    });
                }
            }

            return Ok(new
            {
                GenderDistribution = genderResult,
                ScheduleDistribution = timeIntervals,
                TopConsistence = consistencyData
            });
        }
    }
    public class UpdatedLog
    {
        public long EmployeeId { get; set; } // Use long if EmployeeID needs to be a long
        public int Count { get; set; }
        public DateTime LastAttendance { get; set; }
    }

    public class DashboardInput
    {
        public int Count { get; set; }
        public string Name { get; set; }

    }

}
