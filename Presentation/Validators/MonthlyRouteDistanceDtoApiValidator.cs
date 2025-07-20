using DTO.WebApiDTO.Journey;
using FluentValidation;

namespace Presentation.Validators
{
    public class MonthlyRouteDistanceDtoApiValidator : AbstractValidator<MonthlyRouteDistanceDtoApi>
    {
        public MonthlyRouteDistanceDtoApiValidator()
        {
            RuleFor(x => x.UserId)
                .Must(id => id == null || id != Guid.Empty)
                .WithMessage("UserId must be a valid GUID.");

            RuleFor(x => x.Year)
                .InclusiveBetween(2000, 2100)
                .When(x => x.Year.HasValue)
                .WithMessage("Year must be between 2000 and 2100.");

            RuleFor(x => x.Month)
                .InclusiveBetween(1, 12)
                .When(x => x.Month.HasValue)
                .WithMessage("Month must be between 1 and 12.");
        }
    }
}
