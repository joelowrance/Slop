using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VerdaVidaLawnCare.CoreAPI.Entities;

namespace VerdaVidaLawnCare.CoreAPI.Models;

/// <summary>
/// Types of lawn care services
/// </summary>
public enum ServiceType
{
    Mowing = 1,
    Trimming = 2,
    Edging = 3,
    Fertilization = 4,
    WeedControl = 5,
    Aeration = 6,
    Seeding = 7,
    Mulching = 8,
    LeafRemoval = 9,
    SnowRemoval = 10,
    Other = 99
}

/// <summary>
/// Represents a lawn care service offered
/// </summary>
public class Service : IAuditable
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the service name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the service description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base price for this service
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Gets or sets the type of service
    /// </summary>
    public ServiceType ServiceType { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the record was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the record was last updated
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets whether the service is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property to estimate line items that reference this service
    /// </summary>
    public virtual ICollection<EstimateLineItem> EstimateLineItems { get; set; } = new List<EstimateLineItem>();
}

/// <summary>
/// Entity Type Configuration for Service
/// </summary>
public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.BasePrice)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(s => s.ServiceType)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Create index on ServiceType for filtering
        builder.HasIndex(s => s.ServiceType);

        // Configure relationship with EstimateLineItems
        builder.HasMany(s => s.EstimateLineItems)
            .WithOne(eli => eli.Service)
            .HasForeignKey(eli => eli.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


