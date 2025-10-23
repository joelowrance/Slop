using Microsoft.EntityFrameworkCore;
using VerdaVida.Shared.EntityFrameworkExtensions;
using VerdaVidaLawnCare.CoreAPI.Models;

namespace VerdaVidaLawnCare.CoreAPI.Data;

/// <summary>
/// Seeds the database with initial Equipment and Service data
/// </summary>
public class ApplicationDbContextSeeder : IDbSeeder<ApplicationDbContext>
{
    private readonly ILogger<ApplicationDbContextSeeder> _logger;

    public ApplicationDbContextSeeder(ILogger<ApplicationDbContextSeeder> logger)
    {
        _logger = logger;
    }

    public async Task SeedAsync(ApplicationDbContext context)
    {
        _logger.LogInformation("Starting database seeding for Equipment and Services");

        await SeedEquipmentAsync(context);
        await SeedServicesAsync(context);

        _logger.LogInformation("Database seeding completed successfully");
    }

    private async Task SeedEquipmentAsync(ApplicationDbContext context)
    {
        if (await context.Equipment.AnyAsync())
        {
            _logger.LogInformation("Equipment table already contains data, skipping seeding");
            return;
        }

        var equipmentItems = new List<Equipment>
        {
            new()
            {
                Name = "Commercial Lawn Mower",
                Description = "Professional-grade walk-behind mower for residential and commercial lawn care",
                HourlyRate = 25.00m,
                EquipmentType = EquipmentType.Mower,
                IsActive = true
            },
            new()
            {
                Name = "String Trimmer",
                Description = "Gas-powered string trimmer for precise edge trimming and detail work",
                HourlyRate = 20.00m,
                EquipmentType = EquipmentType.Trimmer,
                IsActive = true
            },
            new()
            {
                Name = "Lawn Edger",
                Description = "Professional edger for creating clean, defined lawn edges",
                HourlyRate = 18.00m,
                EquipmentType = EquipmentType.Edger,
                IsActive = true
            },
            new()
            {
                Name = "Leaf Blower",
                Description = "Commercial-grade backpack blower for efficient debris removal",
                HourlyRate = 15.00m,
                EquipmentType = EquipmentType.Blower,
                IsActive = true
            },
            new()
            {
                Name = "Core Aerator",
                Description = "Heavy-duty core aerator for soil aeration and lawn health improvement",
                HourlyRate = 45.00m,
                EquipmentType = EquipmentType.Aerator,
                IsActive = true
            },
            new()
            {
                Name = "Fertilizer Spreader",
                Description = "Professional broadcast spreader for even fertilizer and seed distribution",
                HourlyRate = 22.00m,
                EquipmentType = EquipmentType.Spreader,
                IsActive = true
            },
            new()
            {
                Name = "Overseeder",
                Description = "Specialized overseeder for lawn renovation and seeding projects",
                HourlyRate = 50.00m,
                EquipmentType = EquipmentType.Seeder,
                IsActive = true
            },
            new()
            {
                Name = "Equipment Trailer",
                Description = "Enclosed trailer for secure equipment transport and storage",
                HourlyRate = 35.00m,
                EquipmentType = EquipmentType.Trailer,
                IsActive = true
            },
            new()
            {
                Name = "Service Truck",
                Description = "Commercial service truck for equipment transport and mobile operations",
                HourlyRate = 75.00m,
                EquipmentType = EquipmentType.Truck,
                IsActive = true
            },
            new()
            {
                Name = "Specialized Equipment",
                Description = "Custom or specialized equipment for unique lawn care needs",
                HourlyRate = 30.00m,
                EquipmentType = EquipmentType.Other,
                IsActive = true
            }
        };

        await context.Equipment.AddRangeAsync(equipmentItems);
        await context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} equipment items", equipmentItems.Count);
    }

    private async Task SeedServicesAsync(ApplicationDbContext context)
    {
        if (await context.Services.AnyAsync())
        {
            _logger.LogInformation("Services table already contains data, skipping seeding");
            return;
        }

        var services = new List<Service>
        {
            new()
            {
                Name = "Lawn Mowing",
                Description = "Regular lawn mowing service with professional equipment and techniques",
                BasePrice = 45.00m,
                ServiceType = ServiceType.Mowing,
                IsActive = true
            },
            new()
            {
                Name = "Trimming Service",
                Description = "Precise trimming around obstacles, fences, and landscape features",
                BasePrice = 35.00m,
                ServiceType = ServiceType.Trimming,
                IsActive = true
            },
            new()
            {
                Name = "Edge Trimming",
                Description = "Professional edge trimming for clean, defined lawn boundaries",
                BasePrice = 40.00m,
                ServiceType = ServiceType.Edging,
                IsActive = true
            },
            new()
            {
                Name = "Fertilization Service",
                Description = "Professional lawn fertilization with appropriate nutrients for optimal growth",
                BasePrice = 85.00m,
                ServiceType = ServiceType.Fertilization,
                IsActive = true
            },
            new()
            {
                Name = "Weed Control",
                Description = "Comprehensive weed control treatment for healthy, weed-free lawns",
                BasePrice = 75.00m,
                ServiceType = ServiceType.WeedControl,
                IsActive = true
            },
            new()
            {
                Name = "Lawn Aeration",
                Description = "Core aeration service to improve soil compaction and root growth",
                BasePrice = 120.00m,
                ServiceType = ServiceType.Aeration,
                IsActive = true
            },
            new()
            {
                Name = "Overseeding Service",
                Description = "Professional overseeding to improve lawn density and appearance",
                BasePrice = 150.00m,
                ServiceType = ServiceType.Seeding,
                IsActive = true
            },
            new()
            {
                Name = "Mulching Service",
                Description = "Mulch installation and maintenance for landscape beds and gardens",
                BasePrice = 65.00m,
                ServiceType = ServiceType.Mulching,
                IsActive = true
            },
            new()
            {
                Name = "Leaf Removal",
                Description = "Complete leaf removal and cleanup service for seasonal maintenance",
                BasePrice = 55.00m,
                ServiceType = ServiceType.LeafRemoval,
                IsActive = true
            },
            new()
            {
                Name = "Snow Removal",
                Description = "Professional snow removal service for driveways and walkways",
                BasePrice = 80.00m,
                ServiceType = ServiceType.SnowRemoval,
                IsActive = true
            },
            new()
            {
                Name = "Custom Service",
                Description = "Specialized lawn care services tailored to unique property needs",
                BasePrice = 100.00m,
                ServiceType = ServiceType.Other,
                IsActive = true
            }
        };

        await context.Services.AddRangeAsync(services);
        await context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} service items", services.Count);
    }
}
