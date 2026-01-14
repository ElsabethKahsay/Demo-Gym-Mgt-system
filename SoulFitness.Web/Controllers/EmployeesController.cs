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
using System.Text;
using SoulFitness.DataObjects;

namespace SoulFitness.Web.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext applicationDbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public EmployeesController(ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager)
        {
            this.applicationDbContext = applicationDbContext;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            return Ok(await applicationDbContext.Employees.Include(e => e.Schedule).Where(e => e.Status == Status.Active).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Employee employee, [FromQuery] int Interval)
        {
            var selectedSchedule = await applicationDbContext.Schedule.FindAsync(Interval);
            employee.Schedule = selectedSchedule;

            if (employee.Schedule == null) return BadRequest("Invalid schedule interval.");

            if (ModelState.IsValid)
            {
                if (!applicationDbContext.Employees.Any(i => i.EmployeeID == employee.EmployeeID))
                {
                    var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var loggedInUser = await userManager.FindByIdAsync(loggedInUserId);
                    
                    employee.FirstName = employee.FirstName.ToTitleCase2();
                    employee.MiddleName = employee.MiddleName?.ToTitleCase2();
                    employee.LastName = employee.LastName?.ToTitleCase2();
                    employee.CreatedBy = loggedInUser?.Email ?? "System";
                    employee.CreatedAt = DateTime.Now;
                    employee.Status = Status.Active;
                    
                    applicationDbContext.Employees.Add(employee);
                    await applicationDbContext.SaveChangesAsync();
                    return CreatedAtAction(nameof(Details), new { id = employee.Id }, employee);
                }
                return BadRequest("Employee ID in use.");
            }
            return BadRequest(ModelState);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var employee = await applicationDbContext.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            employee.Status = Status.Inactive;
            await applicationDbContext.SaveChangesAsync();
            return Ok(new { Message = "Employee deactivated." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(long id, [FromBody] Employee employee, [FromQuery] int Interval)
        {
            if (ModelState.IsValid)
            {
                var selectedSchedule = await applicationDbContext.Schedule.FindAsync(Interval);
                employee.Schedule = selectedSchedule;
                if (employee.Schedule == null) return BadRequest("Invalid schedule interval.");

                try
                {
                    var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var loggedInUser = await userManager.FindByIdAsync(loggedInUserId);
                    employee.UpdatedAt = DateTime.Now;
                    employee.UpdatedBy = loggedInUser?.Email;
                    applicationDbContext.Update(employee);
                    await applicationDbContext.SaveChangesAsync();
                    return Ok(new { Message = "Employee updated." });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!applicationDbContext.Employees.Any(e => e.Id == id)) return NotFound();
                    throw;
                }
            }
            return BadRequest(ModelState);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> Details(long id)
        {
            var employee = await applicationDbContext.Employees.Include(e => e.Schedule).FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null) return NotFound();
            return Ok(employee);
        }

        [HttpPost("import")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Import()
        {
            var attachedFile = Request.Form.Files;
            if (attachedFile.Count == 0) return BadRequest("Please choose a file to import");

            var file = attachedFile[0];
            string fileExt = Path.GetExtension(file.FileName);
            if (!fileExt.Equals(".csv", StringComparison.OrdinalIgnoreCase) && !fileExt.Equals(".txt", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Please choose a proper file format to import");
            }

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var mainList = new List<Employee>();
                while (!reader.EndOfStream)
                {
                    var data = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(data)) continue;
                    var values = data.Split(',');
                    if (values.Length < 7) continue;

                    var tempID = values[1];
                    if (!await applicationDbContext.Employees.AnyAsync(i => i.EmployeeID == tempID) && !mainList.Any(i => i.EmployeeID == tempID))
                    {
                        Employee member = new()
                        {
                            EmployeeID = values[1],
                            FirstName = values[2],
                            CostCenter = values[4],
                            Position = values[5],
                            Status = Status.Active,
                            Gender = values[3].Equals("Female", StringComparison.OrdinalIgnoreCase) ? Gender.Female : Gender.Male,
                            Days = values[7].Equals("GroupA", StringComparison.OrdinalIgnoreCase) ? WeekDays.GroupA : WeekDays.GroupB
                        };

                        var intervals = values[6];
                        member.Schedule = await applicationDbContext.Schedule.FirstOrDefaultAsync(e => e.TimeInterval == intervals);
                        
                        var nameParts = member.FirstName.Split(' ');
                        member.FirstName = nameParts[0].ToTitleCase2();
                        if (nameParts.Length > 1) member.MiddleName = nameParts[1].ToTitleCase2();
                        if (nameParts.Length > 2) member.LastName = nameParts[2].ToTitleCase2();

                        mainList.Add(member);
                    }
                }
                applicationDbContext.Employees.AddRange(mainList);
                await applicationDbContext.SaveChangesAsync();
                return Ok(new { Message = "Members successfully imported." });
            }
        }

        [HttpPost("import-rfid")]
        public async Task<ActionResult> ImportRfid()
        {
            var attachedFile = Request.Form.Files;
            if (attachedFile.Count == 0) return BadRequest("Please choose a file to import");

            var file = attachedFile[0];
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (!reader.EndOfStream)
                {
                    var data = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(data)) continue;
                    var values = data.Split(',');
                    if (values.Length < 2) continue;

                    var tempID = values[0];
                    var currentEmployee = await applicationDbContext.Employees.FirstOrDefaultAsync(i => i.EmployeeID == tempID);
                    if (currentEmployee != null)
                    {
                        currentEmployee.RFID = values[1];
                        applicationDbContext.Employees.Update(currentEmployee);
                    }
                }
                await applicationDbContext.SaveChangesAsync();
                return Ok(new { Message = "RFIDs successfully imported." });
            }
        }

        [HttpGet("attendance")]
        public async Task<ActionResult> GetAttendance(string rfid)
        {
            var today = DateTime.Today;
            var em = await applicationDbContext.Employees.Include(e => e.Schedule)
                .Where(x => x.Status == Status.Active)
                .FirstOrDefaultAsync(e => e.RFID == rfid);

            if (em == null) return NotFound("Member not found or inactive.");

            if (await applicationDbContext.GymAttendance.AnyAsync(x => x.RFID == em.RFID && x.IsReturned == false))
            {
                return BadRequest("Did not return a locker");
            }

            bool isCorrectDayForGroupA = em.Days == WeekDays.GroupA && (today.DayOfWeek == DayOfWeek.Monday || today.DayOfWeek == DayOfWeek.Wednesday || today.DayOfWeek == DayOfWeek.Friday);
            bool isCorrectDayForGroupB = em.Days == WeekDays.GroupB && (today.DayOfWeek == DayOfWeek.Tuesday || today.DayOfWeek == DayOfWeek.Thursday || today.DayOfWeek == DayOfWeek.Saturday);

            if (!isCorrectDayForGroupA && !isCorrectDayForGroupB) return BadRequest("Wrong day.");

            var alreadyAttended = await applicationDbContext.GymAttendance.AnyAsync(x => x.Employees.EmployeeID == em.EmployeeID && x.DateAndTime.Date == today);
            if (alreadyAttended) return BadRequest("Already attended today.");

            var assignmentResult = em.Gender == Gender.Female ? await AssignConsecutiveLocker() : await AssignLockerMen();

            if (assignmentResult.Success)
            {
                var gym = new GymAttendances
                {
                    RFID = em.RFID,
                    EmpID = em.Id,
                    AttendanceStatus = Status.Active,
                    DateAndTime = DateTime.Now,
                    LockerNumber = assignmentResult.LockerNumber ?? 0,
                    IsReturned = false,
                    FloorNumber = assignmentResult.floorNumber
                };
                applicationDbContext.GymAttendance.Add(gym);
                await applicationDbContext.SaveChangesAsync();
                return Ok(new { Message = $"Locker {gym.LockerNumber} on floor {gym.FloorNumber} assigned.", Data = gym });
            }

            return BadRequest("No available lockers.");
        }

        private async Task<LockerAssignmentResult> AssignConsecutiveLocker()
        {
            var locker = await applicationDbContext.Locker
                .Where(l => !l.IsAssigned && l.Gender == Gender.Female && l.LockersStatus == LockersStatus.Functioning)
                .OrderBy(l => l.LockerNumber)
                .FirstOrDefaultAsync();

            if (locker != null)
            {
                locker.IsAssigned = true;
                applicationDbContext.Locker.Update(locker);
                await applicationDbContext.SaveChangesAsync();
                return new LockerAssignmentResult { Success = true, LockerNumber = locker.LockerNumber, floorNumber = locker.Floor };
            }
            return new LockerAssignmentResult { Success = false };
        }

        private async Task<LockerAssignmentResult> AssignLockerMen()
        {
            var locker = await applicationDbContext.Locker
                .Where(l => !l.IsAssigned && l.Gender == Gender.Male && l.LockersStatus == LockersStatus.Functioning)
                .OrderBy(l => l.LockerNumber)
                .FirstOrDefaultAsync();

            if (locker != null)
            {
                locker.IsAssigned = true;
                applicationDbContext.Locker.Update(locker);
                await applicationDbContext.SaveChangesAsync();
                return new LockerAssignmentResult { Success = true, LockerNumber = locker.LockerNumber, floorNumber = locker.Floor };
            }
            return new LockerAssignmentResult { Success = false };
        }

        public class LockerAssignmentResult
        {
            public bool Success { get; set; }
            public int floorNumber { get; set; }
            public int? LockerNumber { get; set; }
        }

        [HttpGet("csv")]
        public async Task<ActionResult> ToCSV()
        {
            var empList = await applicationDbContext.Employees.Include(e => e.Schedule).Where(e => e.Status == Status.Active).ToListAsync();
            string title = DateTime.Today.ToShortDateString() + ".csv";
            var builder = new StringBuilder();
            builder.AppendLine("ID, Name, Gender");
            foreach (var item in empList)
            {
                builder.AppendLine($"{item.EmployeeID}, {item.FullName}, {item.Gender}");
            }
            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", title);
        }
    }
}

public static class Ext
{
    public static string ToTitleCase2(this string value)
    {
        if (value == null) return value;

        String[] words = value.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length == 0) continue;

            Char firstChar = Char.ToUpper(words[i][0]);
            String rest = "";
            if (words[i].Length > 1)
            {
                rest = words[i][1..].ToLower();
            }
            words[i] = firstChar + rest;
        }

        return String.Join(" ", words);

    }
}

