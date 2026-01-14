#nullable disable
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SoulFitness.Abstractions;
using SoulFitness.DataObjects.Data;
using SoulFitness.DataObjects.UserManagment.Identity;
using SoulFitness.DataObjects.ViewModels;
using SoulFitness.Web.Controllers;
using System.Security.Claims;
using Xunit;

namespace SoulFitness.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private readonly Mock<IEmailSender> _mockEmailSender;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly ApplicationDbContext _context;

        public UsersControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "SoulFitnessTest_Users")
                .Options;
            _context = new ApplicationDbContext(options);

            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                new Mock<IUserStore<ApplicationUser>>().Object,
                null, null, null, null, null, null, null, null);

            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                _mockUserManager.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
                null, null, null, null);

            _mockEmailSender = new Mock<IEmailSender>();
            
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Scheme = "http";
            context.Request.Host = new HostString("localhost");
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(context);
        }

        [Fact]
        public async Task CreateUser_ReturnsCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var controller = new UsersController(_context, _mockSignInManager.Object, _mockUserManager.Object, _mockEmailSender.Object, _mockHttpContextAccessor.Object);
            
            // Mocking User property
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            var dto = new UserCreateDto
            {
                User = new ApplicationUser
                {
                    UserName = "testuser",
                    Email = "test@example.com",
                    FirstName = "Test",
                    LastName = "User"
                },
                Roles = new List<string>()
            };

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.Create(dto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("Details", createdAtActionResult.ActionName);
        }

        [Fact]
        public async Task CreateUser_ReturnsBadRequest_WhenUserExists()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "existinguser", FirstName = "Test", LastName = "User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var controller = new UsersController(_context, _mockSignInManager.Object, _mockUserManager.Object, _mockEmailSender.Object, _mockHttpContextAccessor.Object);

            var dto = new UserCreateDto
            {
                User = new ApplicationUser { UserName = "existinguser", FirstName = "Test", LastName = "User" }
            };

            // Act
            var result = await controller.Create(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User already exists.", badRequestResult.Value);
        }
    }
}
