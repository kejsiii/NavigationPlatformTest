using DTO.WebApiDTO.Journey;
using FluentValidation;
using FluentValidation.Results;

namespace Presentation.Validators
{
    public class JourneyFilterRequestDtoApiValidator : AbstractValidator<JourneyFilterRequestDtoApi>
    {
        public JourneyFilterRequestDtoApiValidator()
        {
            RuleFor(x => x.UserId)
                .Must(id => id == null || id != Guid.Empty)
                .WithMessage("UserId must be a valid GUID.");

            RuleFor(x => x.TransportType)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.TransportType))
                .WithMessage("TransportType cannot exceed 100 characters.");

            RuleFor(x => x.StartDateFrom)
                .LessThanOrEqualTo(x => x.StartDateTo)
                .When(x => x.StartDateFrom.HasValue && x.StartDateTo.HasValue)
                .WithMessage("StartDateFrom must be less than or equal to StartDateTo.");

            RuleFor(x => x.ArrivalDateFrom)
                .LessThanOrEqualTo(x => x.ArrivalDateTo)
                .When(x => x.ArrivalDateFrom.HasValue && x.ArrivalDateTo.HasValue)
                .WithMessage("ArrivalDateFrom must be less than or equal to ArrivalDateTo.");
        }
    }
    }

