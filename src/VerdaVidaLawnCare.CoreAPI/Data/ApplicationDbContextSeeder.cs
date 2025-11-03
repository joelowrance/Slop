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
            // Mowing Services
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
                Name = "Premium Lawn Mowing",
                Description = "Premium mowing service with additional cleanup and detail work",
                BasePrice = 60.00m,
                ServiceType = ServiceType.Mowing,
                IsActive = true
            },
            new()
            {
                Name = "Weekly Lawn Mowing",
                Description = "Scheduled weekly mowing service for consistent lawn maintenance",
                BasePrice = 50.00m,
                ServiceType = ServiceType.Mowing,
                IsActive = true
            },

            // Trimming Services
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
                Name = "Detailed Trimming",
                Description = "Comprehensive trimming service for shrubs, hedges, and ornamental plants",
                BasePrice = 45.00m,
                ServiceType = ServiceType.Trimming,
                IsActive = true
            },

            // Edging Services
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
                Name = "Premium Edging",
                Description = "Premium edging service with precision cutting and cleanup",
                BasePrice = 50.00m,
                ServiceType = ServiceType.Edging,
                IsActive = true
            },

            // Fertilization Services
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
                Name = "Organic Fertilization",
                Description = "Eco-friendly organic fertilization service for sustainable lawn care",
                BasePrice = 95.00m,
                ServiceType = ServiceType.Fertilization,
                IsActive = true
            },
            new()
            {
                Name = "Seasonal Fertilization Program",
                Description = "Multi-application seasonal fertilization program for year-round lawn health",
                BasePrice = 300.00m,
                ServiceType = ServiceType.Fertilization,
                IsActive = true
            },

            // Weed Control Services
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
                Name = "Pre-Emergent Weed Control",
                Description = "Preventive weed control treatment applied before weeds germinate",
                BasePrice = 80.00m,
                ServiceType = ServiceType.WeedControl,
                IsActive = true
            },
            new()
            {
                Name = "Spot Weed Treatment",
                Description = "Targeted spot treatment for existing weed problems",
                BasePrice = 50.00m,
                ServiceType = ServiceType.WeedControl,
                IsActive = true
            },

            // Aeration Services
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
                Name = "Aeration with Overseeding",
                Description = "Combined aeration and overseeding service for complete lawn renovation",
                BasePrice = 180.00m,
                ServiceType = ServiceType.Aeration,
                IsActive = true
            },

            // Seeding Services
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
                Name = "New Lawn Seeding",
                Description = "Complete new lawn seeding service with soil preparation and seed application",
                BasePrice = 250.00m,
                ServiceType = ServiceType.Seeding,
                IsActive = true
            },
            new()
            {
                Name = "Patch Seeding",
                Description = "Spot seeding service to repair bare patches and thin areas",
                BasePrice = 80.00m,
                ServiceType = ServiceType.Seeding,
                IsActive = true
            },

            // Mulching Services
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
                Name = "Premium Mulch Installation",
                Description = "High-quality mulch installation with weed barrier and edging",
                BasePrice = 85.00m,
                ServiceType = ServiceType.Mulching,
                IsActive = true
            },

            // Leaf Removal Services
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
                Name = "Leaf Removal with Bagging",
                Description = "Complete leaf removal service with bagging and disposal",
                BasePrice = 70.00m,
                ServiceType = ServiceType.LeafRemoval,
                IsActive = true
            },
            new()
            {
                Name = "Fall Cleanup Service",
                Description = "Comprehensive fall cleanup including leaves, debris, and yard waste removal",
                BasePrice = 100.00m,
                ServiceType = ServiceType.LeafRemoval,
                IsActive = true
            },

            // Snow Removal Services
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
                Name = "Seasonal Snow Removal Contract",
                Description = "Seasonal contract for guaranteed snow removal throughout winter months",
                BasePrice = 400.00m,
                ServiceType = ServiceType.SnowRemoval,
                IsActive = true
            },
            new()
            {
                Name = "Premium Snow Removal",
                Description = "Premium snow removal with ice treatment and thorough cleanup",
                BasePrice = 100.00m,
                ServiceType = ServiceType.SnowRemoval,
                IsActive = true
            },

            // Other Services
            new()
            {
                Name = "Custom Service",
                Description = "Specialized lawn care services tailored to unique property needs",
                BasePrice = 100.00m,
                ServiceType = ServiceType.Other,
                IsActive = true
            },
            new()
            {
                Name = "Lawn Consultation",
                Description = "Professional lawn care consultation and assessment service",
                BasePrice = 75.00m,
                ServiceType = ServiceType.Other,
                IsActive = true
            },
            new()
            {
                Name = "Sprinkler System Maintenance",
                Description = "Sprinkler system inspection, repair, and maintenance service",
                BasePrice = 90.00m,
                ServiceType = ServiceType.Other,
                IsActive = true
            }
        };

        await context.Services.AddRangeAsync(services);
        await context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} service items", services.Count);
    }
}
