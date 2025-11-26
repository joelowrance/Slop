using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VerdaVidaLawnCare.CoreAPI.Entities;

namespace VerdaVidaLawnCare.CoreAPI.Models;

/// <summary>
/// Status of an estimate
/// </summary>
public enum EstimateStatus
{
    Draft = 1,
    Sent = 2,
    Viewed = 3,
    Accepted = 4,
    Rejected = 5,
    Expired = 6,
    Cancelled = 7,
    Completed = 8
}

/// <summary>
/// Represents an estimate for lawn care services
/// </summary>
public class Estimate : IAuditable
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the estimate number (auto-generated)
    /// </summary>
    public string EstimateNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer identifier
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the date the estimate was created
    /// </summary>
    public DateTimeOffset EstimateDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the expiration date of the estimate
    /// </summary>
    public DateTimeOffset ExpirationDate { get; set; }

    /// <summary>
    /// Gets or sets the status of the estimate
    /// </summary>
    public EstimateStatus Status { get; set; } = EstimateStatus.Draft;

    /// <summary>
    /// Gets or sets any notes for the estimate
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the terms and conditions
    /// </summary>
    public string Terms { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scheduled date for the job
    /// </summary>
    public DateTimeOffset? ScheduledDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the job was completed
    /// </summary>
    public DateTimeOffset? CompletedDate { get; set; }

    /// <summary>
    /// Gets or sets notes entered when completing the job
    /// </summary>
    public string CompletionNotes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the record was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the record was last updated
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Navigation property to the customer
    /// </summary>
    public virtual Customer Customer { get; set; } = null!;

    /// <summary>
    /// Navigation property to the estimate line items
    /// </summary>
    public virtual ICollection<EstimateLineItem> EstimateLineItems { get; set; } = new List<EstimateLineItem>();

    /// <summary>
    /// Computed property for total amount (sum of line items)
    /// </summary>
    public decimal TotalAmount => EstimateLineItems?.Sum(li => li.LineTotal) ?? 0;
}

/// <summary>
/// Entity Type Configuration for Estimate
/// </summary>
public class EstimateConfiguration : IEntityTypeConfiguration<Estimate>
{
    public void Configure(EntityTypeBuilder<Estimate> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EstimateNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.CustomerId)
            .IsRequired();

        builder.Property(e => e.EstimateDate)
            .IsRequired();

        builder.Property(e => e.ExpirationDate)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.Terms)
            .HasMaxLength(2000);

        builder.Property(e => e.CompletionNotes)
            .HasMaxLength(2000);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // Create unique index on EstimateNumber
        builder.HasIndex(e => e.EstimateNumber)
            .IsUnique();

        // Create index on CustomerId for faster lookups
        builder.HasIndex(e => e.CustomerId);

        // Create index on EstimateDate for filtering
        builder.HasIndex(e => e.EstimateDate);

        // Create index on Status for filtering
        builder.HasIndex(e => e.Status);

        // Create index on ScheduledDate for filtering
        builder.HasIndex(e => e.ScheduledDate);

        // Create index on CompletedDate for filtering
        builder.HasIndex(e => e.CompletedDate);

        // Configure relationship with Customer
        builder.HasOne(e => e.Customer)
            .WithMany(c => c.Estimates)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure relationship with EstimateLineItems
        builder.HasMany(e => e.EstimateLineItems)
            .WithOne(eli => eli.Estimate)
            .HasForeignKey(eli => eli.EstimateId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore computed property TotalAmount - it's not stored in database
        builder.Ignore(e => e.TotalAmount);
    }
}


