using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VerdaVidaLawnCare.CoreAPI.Entities;

namespace VerdaVidaLawnCare.CoreAPI.Models;

/// <summary>
/// Represents a line item within an estimate
/// </summary>
public class EstimateLineItem : IAuditable
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the estimate identifier
    /// </summary>
    public int EstimateId { get; set; }

    /// <summary>
    /// Gets or sets the service identifier (optional)
    /// </summary>
    public int? ServiceId { get; set; }

    /// <summary>
    /// Gets or sets the equipment identifier (optional)
    /// </summary>
    public int? EquipmentId { get; set; }

    /// <summary>
    /// Gets or sets the line item description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the line total (quantity * unit price)
    /// </summary>
    public decimal LineTotal { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the record was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the record was last updated
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Navigation property to the estimate
    /// </summary>
    public virtual Estimate Estimate { get; set; } = null!;

    /// <summary>
    /// Navigation property to the service (if applicable)
    /// </summary>
    public virtual Service? Service { get; set; }

    /// <summary>
    /// Navigation property to the equipment (if applicable)
    /// </summary>
    public virtual Equipment? Equipment { get; set; }
}

/// <summary>
/// Entity Type Configuration for EstimateLineItem
/// </summary>
public class EstimateLineItemConfiguration : IEntityTypeConfiguration<EstimateLineItem>
{
    public void Configure(EntityTypeBuilder<EstimateLineItem> builder)
    {
        builder.HasKey(eli => eli.Id);

        builder.Property(eli => eli.EstimateId)
            .IsRequired();

        builder.Property(eli => eli.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(eli => eli.Quantity)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(eli => eli.UnitPrice)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(eli => eli.LineTotal)
            .IsRequired()
            .HasPrecision(10, 2);


        builder.Property(eli => eli.CreatedAt)
            .IsRequired();

        // Create index on EstimateId for faster lookups
        builder.HasIndex(eli => eli.EstimateId);

        // Configure relationship with Estimate
        builder.HasOne(eli => eli.Estimate)
            .WithMany(e => e.EstimateLineItems)
            .HasForeignKey(eli => eli.EstimateId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship with Service (optional)
        builder.HasOne(eli => eli.Service)
            .WithMany(s => s.EstimateLineItems)
            .HasForeignKey(eli => eli.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure relationship with Equipment (optional)
        builder.HasOne(eli => eli.Equipment)
            .WithMany(eq => eq.EstimateLineItems)
            .HasForeignKey(eli => eli.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


