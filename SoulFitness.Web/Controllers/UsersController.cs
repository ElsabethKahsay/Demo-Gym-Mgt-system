using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using SoulFitness.DataObjects.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using SoulFitness.Utilities;
using Microsoft.AspNetCore.Identity;
using SoulFitness.DataObjects.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SoulFitness.DataObjects.UserManagment.Services;
using SoulFitness.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Data;
using SoulFitness.DataObjects.UserManagment.Identity;
using SoulFitness.DataObjects;

namespace SoulFitness.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext applicationDbContext;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender emailSender;
        private readonly string baseURL;

        public UsersController(ApplicationDbContext applicationDbContext,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            IHttpContextAccessor httpContext)
        {
            this.applicationDbContext = applicationDbContext;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailSender = emailSender;

            var request = httpContext.HttpContext.Request;
            baseURL = $"{request.Scheme}://{request.Host}";
        }

        [HttpGet]
        public ActionResult<IEnumerable<UserViewModel>> Index()
        {
            var userViewModels = applicationDbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Select(u => new UserViewModel
                {
                    ApplicationUser = u,
                    UserRoles = u.UserRoles.Select(ur => ur.Role).ToList()
                })
                .ToList();

            return Ok(userViewModels);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await signInManager.PasswordSignInAsync(model.Username, model.Password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = await userManager.FindByNameAsync(model.Username);
                return Ok(new { Message = "Login successful", User = user });
            }
            return Unauthorized(new { Message = "Invalid login attempt." });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserViewModel>> Details(string id)
        {
            var user = await applicationDbContext.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null) return NotFound();

            var userRoles = applicationDbContext.UserRoles.Where(f => f.UserId == user.Id).ToList();
            var roles = applicationDbContext.Roles.ToList();
            var selectedRoles = roles.Where(r => userRoles.Any(i => i.RoleId == r.Id)).ToList();

            return Ok(new UserViewModel
            {
                ApplicationUser = user,
                UserRoles = selectedRoles
            });
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] UserCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = dto.User;
            if (await applicationDbContext.Users.AnyAsync(i => i.UserName == user.UserName))
            {
                return BadRequest("User already exists.");
            }

            var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var loggedInUser = loggedInUserId != null ? await userManager.FindByIdAsync(loggedInUserId) : null;

            user.FirstName = user.FirstName.ToTitleCase2();
            user.MiddleName = user.MiddleName?.ToTitleCase2();
            user.LastName = user.LastName.ToTitleCase2();
            user.CreatedBy = loggedInUser?.Email ?? "System";
            user.CreatedAt = DateTime.Now;
            user.status = Status.Active;

            var randomPassword = RandomPasswordGenerator.CreateRandomPassword();
            var result = await userManager.CreateAsync(user, randomPassword);

            if (result.Succeeded)
            {
                if (dto.Roles != null && dto.Roles.Count > 0)
                {
                    foreach (var roleId in dto.Roles)
                    {
                        var role = new ApplicationUserRole { UserId = user.Id, RoleId = roleId };
                        applicationDbContext.Add(role);
                    }
                    await applicationDbContext.SaveChangesAsync();
                }

                string body = $"<p>Dear {user.FullName},</p><p>Login account created. Username: <strong>{user.UserName}</strong>, Password: <strong>{randomPassword}</strong></p>";
                emailSender.SendEmail(body, new List<string> { user.Email }, new List<string>(), "Account Created", "SMS Notification");

                return CreatedAtAction(nameof(Details), new { id = user.Id }, user);
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await userManager.FindByIdAsync(userId);

            if (user != null)
            {
                var result = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    user.FirstLogin = false;
                    await userManager.UpdateAsync(user);
                    return Ok(new { Message = "Password changed successfully." });
                }
                return BadRequest(result.Errors);
            }
            return Unauthorized();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var user = await applicationDbContext.Users.FindAsync(id);
            if (user == null) return NotFound();

            applicationDbContext.Users.Remove(user);
            await applicationDbContext.SaveChangesAsync();
            return Ok(new { Message = "User deleted successfully." });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Edit(string id, [FromBody] ApplicationUser user)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var selectedUser = await applicationDbContext.Users.FindAsync(id);
            if (selectedUser == null) return NotFound();

            var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var loggedInUser = loggedInUserId != null ? await userManager.FindByIdAsync(loggedInUserId) : null;

            selectedUser.UpdatedAt = DateTime.Now;
            selectedUser.UpdatedBy = loggedInUser?.Email;
            selectedUser.Email = user.Email;
            selectedUser.UserName = user.UserName;
            selectedUser.FirstName = user.FirstName;
            selectedUser.MiddleName = user.MiddleName;
            selectedUser.LastName = user.LastName;
            selectedUser.PhoneNumber = user.PhoneNumber;

            applicationDbContext.Users.Update(selectedUser);
            await applicationDbContext.SaveChangesAsync();

            return Ok(new { Message = "User updated successfully." });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await applicationDbContext.Users.FirstOrDefaultAsync(f => f.UserName == model.Username);
            if (user == null) return NotFound("User not found.");

            string code = await userManager.GeneratePasswordResetTokenAsync(user);
            var randomPassword = RandomPasswordGenerator.CreateRandomPassword();

            var result = await userManager.ResetPasswordAsync(user, code, randomPassword);
            if (result.Succeeded)
            {
                string body = $"<p>Dear {user.FullName},</p><p>Password reset. Temporary password: <strong>{randomPassword}</strong></p>";
                emailSender.SendEmail(body, new List<string> { user.Email }, new List<string>(), "Password Reset", "SMS Notification");
                return Ok(new { Message = "Password reset email sent." });
            }
            return BadRequest(result.Errors);
        }

        [HttpPost("logout")]
        public async Task<ActionResult> LogOff()
        {
            await signInManager.SignOutAsync();
            return Ok(new { Message = "Logged out successfully." });
        }
    }
}
