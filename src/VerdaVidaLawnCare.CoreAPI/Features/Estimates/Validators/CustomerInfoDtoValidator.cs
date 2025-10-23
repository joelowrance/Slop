using FluentValidation;
using VerdaVidaLawnCare.CoreAPI.Features.Estimates.DTOs;

namespace VerdaVidaLawnCare.CoreAPI.Features.Estimates.Validators;

/// <summary>
/// Validator for CustomerInfoDto
/// </summary>
public class CustomerInfoDtoValidator : AbstractValidator<CustomerInfoDto>
{
    public CustomerInfoDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("Customer first name is required")
            .MaximumLength(100)
            .WithMessage("First name cannot exceed 200 characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Customer last name is required")
            .MaximumLength(100)
            .WithMessage("Last name cannot exceed 200 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Customer email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Customer phone is required")
            .MaximumLength(20)
            .WithMessage("Phone cannot exceed 20 characters");

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("Customer address is required")
            .MaximumLength(500)
            .WithMessage("Address cannot exceed 500 characters");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required")
            .MaximumLength(100)
            .WithMessage("City cannot exceed 100 characters");

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("State is required")
            .MaximumLength(50)
            .WithMessage("State cannot exceed 50 characters");

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .WithMessage("Postal code is required")
            .MaximumLength(20)
            .WithMessage("Postal code cannot exceed 20 characters");
    }
}


