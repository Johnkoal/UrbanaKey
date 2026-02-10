using FluentValidation;
using UrbanaKey.Api; // Assuming OnboardingRequest is here or implicitly available if top-level in Program.cs
// However, OnboardingRequest is a record at the bottom of Program.cs. 
// It might be better to move OnboardingRequest to a separate file or just keep it there. 
// If it's in Program.cs top-level (no namespace), it's in the global namespace or the Program's namespace.
// Let's check Program.cs again. It has `public record OnboardingRequest`.

namespace UrbanaKey.Api.Validators;

public class OnboardingRequestValidator : AbstractValidator<OnboardingRequest>
{
    public OnboardingRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FullName).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
