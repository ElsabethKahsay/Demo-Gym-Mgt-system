using FluentValidation;
using SoulFitness.DataObjects.ViewModels;

namespace SoulFitness.DataObjects.Validators
{
    public class UserCreateDtoValidator : AbstractValidator<UserCreateDto>
    {
        public UserCreateDtoValidator()
        {
            RuleFor(x => x.User).NotNull().WithMessage("User information is required");
            RuleFor(x => x.User.UserName).NotEmpty().MinimumLength(3);
            RuleFor(x => x.User.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.User.FirstName).NotEmpty();
            RuleFor(x => x.User.LastName).NotEmpty();
            RuleFor(x => x.Roles).NotEmpty().WithMessage("At least one role must be assigned");
        }
    }
}
