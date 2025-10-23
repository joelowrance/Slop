using FluentValidation;
using VerdaVidaLawnCare.CoreAPI.Features.Estimates.DTOs;

namespace VerdaVidaLawnCare.CoreAPI.Features.Estimates.Validators;

/// <summary>
/// Validator for EstimateLineItemDto
/// </summary>
public class EstimateLineItemDtoValidator : AbstractValidator<EstimateLineItemDto>
{
    public EstimateLineItemDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Line item description is required")
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Unit price cannot be negative");

        RuleFor(x => x.LineTotal)
            .Equal(x => x.Quantity * x.UnitPrice)
            .WithMessage("Line total must equal quantity * unit price");

        RuleFor(x => x)
            .Must(x => x.ServiceId.HasValue || x.EquipmentId.HasValue || !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("Line item must reference a service, equipment, or have a description");
    }
}


