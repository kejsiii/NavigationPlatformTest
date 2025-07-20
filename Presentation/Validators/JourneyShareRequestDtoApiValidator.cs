using DTO.WebApiDTO.Journey;
using FluentValidation;

namespace Presentation.Validators
{
    public class JourneyShareRequestDtoApiValidator : AbstractValidator<JourneyShareRequestDtoApi>
    {
        public JourneyShareRequestDtoApiValidator()
        {
            RuleFor(x => x.UserIds)
                .NotNull().WithMessage("UserIds list cannot be null.")
                .NotEmpty().WithMessage("At least one user ID must be provided.");

            RuleForEach(x => x.UserIds)
                .NotEqual(Guid.Empty).WithMessage("User ID cannot be an empty GUID.");

            RuleFor(x => x.UserIds)
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage("Duplicate user IDs are not allowed.");
        }
    }
}
