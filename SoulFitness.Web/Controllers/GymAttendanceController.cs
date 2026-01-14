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


namespace SoulFitness.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GymAttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext applicationDbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public GymAttendanceController(ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager)
        {
            this.applicationDbContext = applicationDbContext;
            this.userManager = userManager;
        }

        [HttpGet("today")]
        public async Task<ActionResult<IEnumerable<GymAttendances>>> GetTodayAttendance([FromQuery] string searchingField = null)
        {
            var today = DateTime.Today;
            if (string.IsNullOrEmpty(searchingField))
            {
                var list = await applicationDbContext.GymAttendance.Include(e => e.Employees)
                    .Where(e => e.DateAndTime.Date == today)
                    .ToListAsync();
                return Ok(list);
            }

            var em = await applicationDbContext.Employees.FirstOrDefaultAsync(e => e.RFID == searchingField || e.EmployeeID == searchingField);
            if (em == null) return NotFound("Member not found.");

            var attendance = await applicationDbContext.GymAttendance.Include(e => e.Employees)
                .Where(e => e.Employees.Id == em.Id && e.DateAndTime.Date == today)
                .ToListAsync();
            return Ok(attendance);
        }

        [HttpPost("return-locker")]
        public async Task<ActionResult> ReturnLocker([FromQuery] string rfid)
        {
            var today = DateTime.Today;
            var em = await applicationDbContext.Employees.FirstOrDefaultAsync(e => e.RFID == rfid);
            if (em == null) return NotFound("Member not found.");

            var tempAttendance = await applicationDbContext.GymAttendance
                .Where(x => x.RFID == rfid && x.DateAndTime.Date == today)
                .FirstOrDefaultAsync();

            if (tempAttendance != null)
            {
                var locker = await applicationDbContext.Locker.FirstOrDefaultAsync(x => x.LockerNumber == tempAttendance.LockerNumber);
                if (locker != null)
                {
                    locker.IsAssigned = false;
                    applicationDbContext.Locker.Update(locker);
                }

                tempAttendance.IsReturned = true;
                tempAttendance.ReturnDateAndTime = DateTime.Now;
                applicationDbContext.GymAttendance.Update(tempAttendance);
                await applicationDbContext.SaveChangesAsync();
                return Ok(new { Message = "Locker returned successfully." });
            }

            var lateAttendance = await applicationDbContext.GymAttendance.Include(x => x.Employees)
                .Where(x => x.RFID == rfid && x.IsReturned == false)
                .FirstOrDefaultAsync();

            if (lateAttendance != null)
            {
                var locker = await applicationDbContext.Locker.FirstOrDefaultAsync(x => x.LockerNumber == lateAttendance.LockerNumber && x.Floor == lateAttendance.FloorNumber && x.Gender == lateAttendance.Employees.Gender);
                if (locker != null)
                {
                    locker.IsAssigned = false;
                    applicationDbContext.Locker.Update(locker);
                }

                lateAttendance.IsReturned = true;
                lateAttendance.ReturnDateAndTime = DateTime.Now;
                
                MisconductLog misconduct = new()
                {
                    FirstName = em.FirstName,
                    LastName = em.MiddleName,
                    EmployeeId = em.EmployeeID,
                    MisconductType = "Locker return",
                    MisconductStatus = MisconductStatus.First_Warning,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "Automatic",
                    Notes = "Did not return locker on time"
                };

                applicationDbContext.GymAttendance.Update(lateAttendance);
                applicationDbContext.MisconductLog.Add(misconduct);
                await applicationDbContext.SaveChangesAsync();
                return Ok(new { Message = "Late locker return processed with misconduct log." });
            }

            return BadRequest("No active locker session found for this member.");
        }

        [HttpGet("master")]
        public async Task<ActionResult<IEnumerable<GymAttendances>>> MasterData([FromQuery] string searchingField = null)
        {
            IQueryable<GymAttendances> query = applicationDbContext.GymAttendance.Include(e => e.Employees).Where(e => e.AttendanceStatus == Status.Active);
            if (!string.IsNullOrEmpty(searchingField))
            {
                query = query.Where(e => e.Employees.EmployeeID == searchingField);
            }
            else
            {
                query = query.Take(50);
            }
            return Ok(await query.ToListAsync());
        }

        [HttpGet("csv/daily")]
        public async Task<ActionResult> ToCSVDaily()
        {
            var date = DateTime.Today.Date;
            var emp = await applicationDbContext.GymAttendance.Include(e => e.Employees).Where(e => e.DateAndTime.Date == date).ToListAsync();
            return ConvertToCSV(emp, $"{date:yyyy-MM-dd}_Daily.csv");
        }

        [HttpGet("csv/range")]
        public async Task<ActionResult> ToCSVRange([FromQuery] string from, [FromQuery] string to)
        {
            if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
            {
                return BadRequest("Invalid date format. Use YYYY-MM-DD.");
            }

            var emp = await applicationDbContext.GymAttendance.Include(e => e.Employees)
                .Where(e => e.DateAndTime >= fromDate && e.DateAndTime < toDate)
                .ToListAsync();

            var distinctItems = emp.GroupBy(x => x.Employees.EmployeeID).Select(y => y.First()).ToList();
            return ConvertToCSV(distinctItems, $"{from}_to_{to}.csv");
        }

        private ActionResult ConvertToCSV(List<GymAttendances> emp, string fileName)
        {
            var builder = new StringBuilder();
            builder.AppendLine("ID, Name, Locker, CostCenter, DateTime");
            foreach (var item in emp)
            {
                builder.AppendLine($"{item.Employees.EmployeeID}, {item.Employees.FullName}, {item.LockerNumber}, {item.Employees.CostCenter}, {item.DateAndTime}");
            }
            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", fileName);
        }
    }
}


