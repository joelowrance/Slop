using FluentValidation;
using VerdaVidaLawnCare.CoreAPI.Data;
using VerdaVidaLawnCare.CoreAPI.Features.Estimates;
using VerdaVidaLawnCare.CoreAPI.Features.Estimates.DTOs;

namespace VerdaVidaLawnCare.CoreAPI.Features.Estimates.Validators;

/// <summary>
/// Validator for SubmitEstimateCommand
/// </summary>
public class SubmitEstimateCommandValidator : AbstractValidator<SubmitEstimateCommand>
{
    public SubmitEstimateCommandValidator(ApplicationDbContext context)
    {
        RuleFor(x => x.Request.Customer)
            .NotNull()
            .WithMessage("Customer information is required")
            .SetValidator(new CustomerInfoDtoValidator());

        RuleFor(x => x.Request.LineItems)
            .NotNull()
            .WithMessage("Line items are required")
            .NotEmpty()
            .WithMessage("At least one line item is required");

        RuleForEach(x => x.Request.LineItems)
            .SetValidator(new EstimateLineItemDtoValidator(context));

        RuleFor(x => x.Request.Notes)
            .MaximumLength(2000)
            .WithMessage("Notes cannot exceed 2000 characters");

        RuleFor(x => x.Request.Terms)
            .MaximumLength(2000)
            .WithMessage("Terms cannot exceed 2000 characters");

        RuleFor(x => x.Request.ExpirationDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.Request.ExpirationDate.HasValue)
            .WithMessage("Expiration date must be in the future");
    }
}
