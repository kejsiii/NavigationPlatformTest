using DTO.WebApiDTO.User;
using FluentValidation;

namespace Presentation.Validators
{
    public class LoginRequestDtoApiValidator : AbstractValidator<LoginRequestDtoApi>
    {
        public LoginRequestDtoApiValidator()
        {
            RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");


        }
    }
}
