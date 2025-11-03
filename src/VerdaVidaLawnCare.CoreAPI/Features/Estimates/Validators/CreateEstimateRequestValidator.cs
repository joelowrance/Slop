using FluentValidation;
using VerdaVidaLawnCare.CoreAPI.Data;
using VerdaVidaLawnCare.CoreAPI.Features.Estimates.DTOs;

namespace VerdaVidaLawnCare.CoreAPI.Features.Estimates.Validators;

/// <summary>
/// Validator for CreateEstimateRequest
/// </summary>
public class CreateEstimateRequestValidator : AbstractValidator<CreateEstimateRequest>
{
    public CreateEstimateRequestValidator(ApplicationDbContext context)
    {
        RuleFor(x => x.Customer)
            .NotNull()
            .WithMessage("Customer information is required")
            .SetValidator(new CustomerInfoDtoValidator());

        RuleFor(x => x.LineItems)
            .NotNull()
            .WithMessage("Line items are required")
            .NotEmpty()
            .WithMessage("At least one line item is required");

        RuleForEach(x => x.LineItems)
            .SetValidator(new EstimateLineItemDtoValidator(context));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Notes cannot exceed 2000 characters");

        RuleFor(x => x.Terms)
            .MaximumLength(2000)
            .WithMessage("Terms cannot exceed 2000 characters");

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.ExpirationDate.HasValue)
            .WithMessage("Expiration date must be in the future");
    }
}
