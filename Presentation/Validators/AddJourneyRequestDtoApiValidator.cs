using DTO.WebApiDTO.User;
using FluentValidation;

namespace Presentation.Validators
{
    public class AddJourneyRequestDtoApiValidator : AbstractValidator<AddJourneyRequestDtoApi>
    {
        public AddJourneyRequestDtoApiValidator()
        {
            RuleFor(x => x.StartingLocation)
                .NotEmpty().WithMessage("Starting location is required.")
                .MaximumLength(100).WithMessage("Starting location cannot exceed 100 characters.");

            RuleFor(x => x.ArrivalLocation)
                .NotEmpty().WithMessage("Arrival location is required.")
                .MaximumLength(100).WithMessage("Arrival location cannot exceed 100 characters.");

            RuleFor(x => x.TransportationType)
                .NotEmpty().WithMessage("Transportation type is required.")
                .MaximumLength(50).WithMessage("Transportation type cannot exceed 50 characters.");

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("Start time is required.")
                .Must(BeAValidDate).WithMessage("Start time must be a valid date.")
                .LessThan(x => x.ArrivalTime).WithMessage("Start time must be before arrival time.");

            RuleFor(x => x.ArrivalTime)
                .NotEmpty().WithMessage("Arrival time is required.")
                .Must(BeAValidDate).WithMessage("Arrival time must be a valid date.");

            RuleFor(x => x.RouteDistanceKm)
                .GreaterThan(0).WithMessage("Route distance must be greater than zero.");
        }

        private bool BeAValidDate(DateTime date)
        {
            return date != default;
        }
    }
}
