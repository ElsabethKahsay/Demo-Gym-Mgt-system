using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoulFitness.Abstractions;
using SoulFitness.DataObjects;
using SoulFitness.DataObjects.Data;
using SoulFitness.DataObjects.UserManagment.Identity;
using SoulFitness.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SoulFitness.Web.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CoachesController : ControllerBase
    {
        private readonly ApplicationDbContext applicationDbContext;
        private readonly IEmailSender emailSender;
        public CoachesController(ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> _userManager, IEmailSender emailSender)
        {
            this.emailSender = emailSender;
            this.applicationDbContext = applicationDbContext;
            this.userManager = _userManager;
        }
        public UserManager<ApplicationUser> userManager { get; private set; }

        // GET: api/Coaches
        [HttpGet]
        public ActionResult<IEnumerable<Coaches>> Index()
        {
            return Ok(applicationDbContext.Coaches.Where(x => x.Status == Status.Active).ToList());
        }

        [HttpGet("attendance")]
        public async Task<ActionResult<IEnumerable<CoachesAttendance>>> CoachesAttendance(string? rfid)
        {
            List<CoachesAttendance> gymAttendances = new();
         
            var today = DateTime.Today;
            var employee = await applicationDbContext.CoachesAttendance.Include(x=>x.Coach).Where(e => e.DateAndTime.Date == today).ToListAsync();

            if (rfid != null)
            {
                var em = await applicationDbContext.Coaches
               .FirstOrDefaultAsync(e => e.RFID == rfid);

                var gymList = await applicationDbContext.CoachesAttendance
                    .Include(e => e.Coach).Where(x => x.DateAndTime.Date == today)
                    .ToListAsync();

                if (em != null && em.Status == Status.Active)
                {
                   if (!gymList.Any(i => i.Coach.EmployeeId == em.EmployeeId))
                    {
                        TimeSpan currentTime = DateTime.Now.TimeOfDay;
                        DaysOfTheWeek customDayOfWeek= (DaysOfTheWeek)today.DayOfWeek; 
                        var tenMinutes = new TimeSpan(0, 10, 0);
                        var currentTimeEnd = currentTime + tenMinutes;

                        var schedules = await applicationDbContext.CoachesSchedule.Include(x=>x.coach)
                            .Where(x => x.Day == customDayOfWeek )
                            .ToListAsync();

                        bool onTime = schedules.Any(x =>x.coach.RFID == rfid &&
                            x.ScheduleStart <= currentTime &&
                            x.ScheduleStart.Add(tenMinutes) >= currentTime);

                        bool late = schedules.Any(x =>
                           x.coach.RFID == rfid &&
                           x.ScheduleEnd > currentTime &&
                           x.ScheduleStart.Add(tenMinutes) < currentTimeEnd
                        );

                        if (onTime)
                        {
                            var gym = new CoachesAttendance();
                            gym.RFID = em.RFID;
                            gym.CoachId = em.id;
                            gym.DateAndTime = DateTime.Now;
                            gymAttendances.Add(gym);
                            applicationDbContext.CoachesAttendance.AddRange(gymAttendances);
                            await applicationDbContext.SaveChangesAsync();
                            return Ok(new { Message = "Scanned.", Data = gymList });
                        }
                        else if (late)
                        {
                            var gym = new CoachesAttendance();
                            gym.RFID = em.RFID;
                            gym.CoachId = em.id;
                            gym.DateAndTime = DateTime.Now;
                            gymAttendances.Add(gym);
                            var subject = "Late coach";
                            var message = "Coach" + em.FMName + " was late for more than 10 minutes today";
                            string notificationType = "Notification";
                            List<string> receiver = new List<string> { "YonasAdu@ethiopianairlines.com" };
                            List<string> emailsInCopy = new List<string> { "SememenY@ethiopianairlines.com", "hizbawits@ethiopianairlines.com" };
                            
                            emailSender.SendEmail(message, receiver, emailsInCopy, subject, notificationType);

                            applicationDbContext.CoachesAttendance.AddRange(gymAttendances);
                            await applicationDbContext.SaveChangesAsync();
                            return Ok(new { Message = "You are late. An automatic email has been sent to your boss", Data = gymList });
                        }
                        else
                        {
                            return BadRequest(new { Message = em.FMName + " Now is not your schedule" });
                        }
                   }
                    else if (gymList.Any(x => x.RFID == rfid))
                    {
                        return BadRequest(new { Message = "Id already scanned" });
                    }
                }
                else
                {
                    return NotFound(new { Message = "Coach not found" });
                }
            }
            
            return Ok(employee);           
        }

        // POST: api/Coaches
        [HttpPost]
        public async Task<ActionResult<Coaches>> Create([FromBody] Coaches coach)
        {
            if (ModelState.IsValid)
            {
                if (applicationDbContext.Coaches.Any(e => e.EmployeeId == coach.EmployeeId))
                {
                    return BadRequest("Employee exists.");
                }
                var loggedInUserId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (loggedInUserId != null)
                {
                    coach.CreatedBy = loggedInUserId;
                    coach.CreatedAt = DateTime.Now;
                    coach.Status = Status.Active;
                    applicationDbContext.Coaches.Add(coach);
                    await applicationDbContext.SaveChangesAsync();
                    return CreatedAtAction(nameof(Index), new { id = coach.id }, coach);
                }
                return Unauthorized();
            }
            return BadRequest(ModelState);
        }

        [HttpPost("schedule")]
        public async Task<ActionResult> CreateSchedule(CoachesSchedule coach, int Interval)
        {
            if (ModelState.IsValid)
            {
                if (coach.CoachId == 0 && Interval == 0)
                {
                    return BadRequest("Coach ID is required.");
                }
                if (applicationDbContext.CoachesSchedule.Any(e => e.CoachId == Interval && e.Day==coach.Day))
                {
                    return BadRequest("Schedule exists.");
                }
                var loggedInUserId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                coach.CreatedBy = loggedInUserId;
                coach.CreatedAt = DateTime.Now;
                coach.CoachId = Interval;
                applicationDbContext.CoachesSchedule.Add(coach);
                await applicationDbContext.SaveChangesAsync();
                return Ok(new { Message = "New schedule successfully added." });
            }
            return BadRequest(ModelState);
        }

        [HttpPut("schedule/{id}")]
        public async Task<IActionResult> EditSchedule(long id, [FromBody] CoachesSchedule coachesSchedule, int coach)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (applicationDbContext.CoachesSchedule.Any(e => e.CoachId == coach && e.Day == coachesSchedule.Day && e.ScheduleEnd == coachesSchedule.ScheduleEnd && e.ScheduleStart == coachesSchedule.ScheduleStart))
                    {
                        return BadRequest("Schedule exists.");
                    }
                    var loggedInUserId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var loggedInUser = await userManager.FindByIdAsync(loggedInUserId);
                    coachesSchedule.UpdatedAt = DateTime.Now;                   
                    coachesSchedule.UpdatedBy = loggedInUser?.Email;
                    coachesSchedule.CoachId = coach;
                    applicationDbContext.Update(coachesSchedule);
                    await applicationDbContext.SaveChangesAsync();
                    return Ok(new { Message = "User successfully edited." });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!applicationDbContext.CoachesSchedule.Any(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            return BadRequest(ModelState);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] Coaches coach)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var loggedInUserId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var loggedInUser = await userManager.FindByIdAsync(loggedInUserId);
                    coach.UpdatedAt = DateTime.Now;
                    coach.UpdatedBy = loggedInUser?.Email;
                    coach.Status = Status.Active;
                    applicationDbContext.Update(coach);
                    await applicationDbContext.SaveChangesAsync();
                    return Ok(new { Message = "coach successfully edited." });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!applicationDbContext.Coaches.Any(e => e.id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            return BadRequest(ModelState);
        }

        [HttpGet("csv")]
        public async Task<ActionResult> ToCSV(string From, string To)
        {
            if (From != null && To != null)
            {
                DateTime From1 = Convert.ToDateTime(From);
                var To1 = Convert.ToDateTime(To);

                var emp = await applicationDbContext.CoachesAttendance.Include(e => e.Coach).Where(e => e.DateAndTime >= From1 && e.DateAndTime < To1).ToListAsync();

                var distinctItems = emp.GroupBy(x => x.Coach.EmployeeId).Select(y => y.First()).ToList();
                string title = From + " to" + To + ".csv";
                var builder = new StringBuilder();
                builder.AppendLine("ID, Name, DateTime");
                foreach (var item in distinctItems)
                {
                    builder.AppendLine($"{item.Coach.EmployeeId}, {item.Coach.FMName}, {item.DateAndTime}");
                }

                return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", title);
            }
            return BadRequest("Please choose a proper date format");
        }

        [HttpDelete("schedule/{id}")]
        public async Task<IActionResult> DeleteSchedule(long id)
        {
            var sched = await applicationDbContext.CoachesSchedule.FindAsync(id);
            if (sched == null) return NotFound();
            
            applicationDbContext.Remove(sched);
            await applicationDbContext.SaveChangesAsync();
            return Ok(new { Message = "Schedule deleted." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await applicationDbContext.Coaches.FindAsync(id);
            if (employee == null) return NotFound();
            
            employee.Status = Status.Inactive;
            await applicationDbContext.SaveChangesAsync();
            return Ok(new { Message = "coach successfully delimited." });
        }

        [HttpGet("schedule")]
        public async Task<ActionResult<IEnumerable<CoachesSchedule>>> GetSchedule()
        {
            return Ok(await applicationDbContext.CoachesSchedule.Include(e => e.coach).OrderBy(x=>x.Day).ThenBy(x=>x.ScheduleStart).ToListAsync()); 
        }

        [HttpPost("import-schedule")]
        public async Task<ActionResult> ImportSchedule()
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
                while (!reader.EndOfStream)
                {
                    var data = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(data)) continue;
                    var values = data.Split(',');
                    if (values.Length < 4) continue;

                    if (Enum.TryParse(values[0], true, out DaysOfTheWeek dayOfWeek))
                    {
                        var timeString1 = values[1].Trim();
                        var timeString2 = values[2].Trim();
                        if (DateTime.TryParseExact(timeString1, "hh:mmtt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime) && 
                            DateTime.TryParseExact(timeString2, "hh:mmtt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime2))
                        {
                            var coach = await applicationDbContext.Coaches.FirstOrDefaultAsync(i => i.EmployeeId == Convert.ToInt32(values[3]));
                            if (coach == null) continue;

                            CoachesSchedule item = new()
                            {
                                CoachId = coach.id,
                                Day = dayOfWeek,
                                ScheduleStart = dateTime.TimeOfDay,
                                ScheduleEnd = dateTime2.TimeOfDay,
                                CreatedAt = DateTime.Now,
                                CreatedBy = User.Identity?.Name ?? "System"
                            };
                            applicationDbContext.CoachesSchedule.Add(item);
                        }
                    }
                }
                await applicationDbContext.SaveChangesAsync();
                return Ok(new { Message = "Schedule successfully imported." });
            }
        }
    }
}
