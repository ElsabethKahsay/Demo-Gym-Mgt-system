#nullable disable
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SoulFitness.DataObjects;
using SoulFitness.DataObjects.Data;
using SoulFitness.DataObjects.UserManagment.Identity;
using SoulFitness.Web.Controllers;
using System.Security.Claims;
using Xunit;

namespace SoulFitness.Tests.Controllers
{
    public class SchedulesControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly ApplicationDbContext _context;

        public SchedulesControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "SoulFitnessTest_Schedules")
                .Options;
            _context = new ApplicationDbContext(options);

            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                new Mock<IUserStore<ApplicationUser>>().Object,
                null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task CreateSchedule_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var controller = new SchedulesController(_context, _mockUserManager.Object);
            
            // Mocking User property in Controller
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, "1"),
            }));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = user }
            };

            var schedule = new Schedules
            {
                TimeInterval = "08:00 - 10:00",
                Description = "Morning Session",
                Limit = 20,
                status = Status.Active
            };

            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser { Email = "admin@example.com" });

            // Act
            var result = await controller.Create(schedule);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("Details", createdAtActionResult.ActionName);
            
            var savedSchedule = await _context.Schedule.FirstOrDefaultAsync(s => s.TimeInterval == "08:00 - 10:00");
            Assert.NotNull(savedSchedule);
            Assert.Equal("admin@example.com", savedSchedule.CreatedBy);
        }

        [Fact]
        public async Task CreateSchedule_ReturnsBadRequest_WhenIntervalExists()
        {
            // Arrange
            _context.Schedule.Add(new Schedules { TimeInterval = "Existing", Description = "Test", Limit = 10, status = Status.Active });
            await _context.SaveChangesAsync();

            var controller = new SchedulesController(_context, _mockUserManager.Object);

            // Act
            var result = await controller.Create(new Schedules { TimeInterval = "Existing", Description = "Test", Limit = 10 });

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Schedule interval already exists.", badRequestResult.Value);
        }
    }
}
