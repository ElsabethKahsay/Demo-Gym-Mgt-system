using FluentValidation;
using SoulFitness.DataObjects;

namespace SoulFitness.DataObjects.Validators
{
    public class SchedulesValidator : AbstractValidator<Schedules>
    {
        public SchedulesValidator()
        {
            RuleFor(x => x.TimeInterval).NotEmpty().WithMessage("Time interval is required");
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.Limit).GreaterThan(0).WithMessage("Limit must be greater than 0");
        }
    }
}
