using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace SoulFitness.DataObjects.UserManagment.Identity
{
    // Models used as parameters to AccountController actions.
    public class ChangePasswordBindingModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordResBO : ValidationError
    {

    }
    public class ChangePasswordBindingModelValidator : AbstractValidator<ChangePasswordBindingModel>
    {
        public ChangePasswordBindingModelValidator()
        {
            RuleFor(x => x.NewPassword).NotEmpty().WithMessage("Password is required");
            RuleFor(x => x.OldPassword).NotEmpty().WithMessage("Old password is required");
            RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage("Confirm password is required");
        }
    }

    public class LoginViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        //[Required]
        //[EmailAddress]
        //[Display(Name = "Email")]
        //public string Email { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        public string GoogleCaptchaToken { get; set; }
    }

    public class RoleViewModel
    {
        [Required]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; }
        public string Description { get; set; }

        public RoleViewModel() { }
        public RoleViewModel(ApplicationRole role)
        {
            this.RoleName = role.Name;
            this.Description = role.Description;
        }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ResetPasswordModelValidator : AbstractValidator<ResetPasswordViewModel>
    {
        public ResetPasswordModelValidator()
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage("User name is required.");
            //RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid email address.");

            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
            RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage("Confirm Password is required");
            RuleFor(x => x.Code).NotEmpty().WithMessage("Code is required");
        }
    }

    public class RegisterBindingModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

    }

    public class RegisterBindingModelValidator : AbstractValidator<RegisterBindingModel>
    {
        public RegisterBindingModelValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.");
            RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid email address.");

            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
            RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage("Confirm Password is required");
        }
    }
}
