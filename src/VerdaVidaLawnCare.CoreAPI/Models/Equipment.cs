using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VerdaVidaLawnCare.CoreAPI.Entities;

namespace VerdaVidaLawnCare.CoreAPI.Models;

/// <summary>
/// Types of lawn care equipment
/// </summary>
public enum EquipmentType
{
    Mower = 1,
    Trimmer = 2,
    Edger = 3,
    Blower = 4,
    Aerator = 5,
    Spreader = 6,
    Seeder = 7,
    Trailer = 8,
    Truck = 9,
    Other = 99
}

/// <summary>
/// Represents equipment used for lawn care services
/// </summary>
public class Equipment : IAuditable
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the equipment name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the equipment description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hourly rate for this equipment
    /// </summary>
    public decimal HourlyRate { get; set; }

    /// <summary>
    /// Gets or sets the type of equipment
    /// </summary>
    public EquipmentType EquipmentType { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the record was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the record was last updated
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets whether the equipment is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property to estimate line items that reference this equipment
    /// </summary>
    public virtual ICollection<EstimateLineItem> EstimateLineItems { get; set; } = new List<EstimateLineItem>();
}

/// <summary>
/// Entity Type Configuration for Equipment
/// </summary>
public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.HourlyRate)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(e => e.EquipmentType)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Create index on EquipmentType for filtering
        builder.HasIndex(e => e.EquipmentType);

        // Configure relationship with EstimateLineItems
        builder.HasMany(e => e.EstimateLineItems)
            .WithOne(eli => eli.Equipment)
            .HasForeignKey(eli => eli.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


