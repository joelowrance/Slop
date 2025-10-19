using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VerdaVidaLawnCare.CoreAPI.Entities;
using VerdaVidaLawnCare.CoreAPI.Models;

namespace VerdaVidaLawnCare.CoreAPI.Data;

public class ApplicationDbContext : DbContext
{
    private readonly ILogger<ApplicationDbContext> _logger;
    // JSON options for consistent serialization

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ILogger<ApplicationDbContext> logger) : base(options)
    {
        _logger = logger;
    }

    // DbSets for all entities
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Service> Services { get; set; } = null!;
    public DbSet<Equipment> Equipment { get; set; } = null!;
    public DbSet<Estimate> Estimates { get; set; } = null!;
    public DbSet<EstimateLineItem> EstimateLineItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _logger.LogDebug("Configuring Job domain model");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        ApplyGlobalFilters(modelBuilder);
        ConfigureConventions(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Enable sensitive data logging in development
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }

        // Configure query tracking behavior
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);

        // Suppress the pending model changes warning since we manually created Equipment tables
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Add audit information before saving
            AddAuditInformation();

            // Validate tenant isolation
            // ValidateTenantIsolation(); // here we would ensure tenant id is set (like audit info)

            var result = await base.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Successfully saved {ChangeCount} changes to database", result);
            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency conflict occurred while saving changes");
            throw new VerdaVida.Shared.Exceptions.ConcurrencyException(
                "A concurrency conflict occurred. The record may have been modified by another user.", ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update failed while saving changes");
            throw new VerdaVida.Shared.Exceptions.PersistenceException(
                "Failed to save changes to the database.", ex);
        }
    }

    private void AddAuditInformation()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditable && (e.State == EntityState.Added || e.State == EntityState.Modified))
            .ToList();

        foreach (var entry in entries)
        {
            if (entry.Entity is IAuditable _)
            {
                if (entry.State == EntityState.Modified)
                {
                    // Ensure UpdatedAt is current for any modifications
                    // Note: In a pure DDD approach, this should be handled by domain operations
                    // This is a safety net for cases where domain operations might miss it
                    var updatedAtProperty = entry.Property(nameof(IAuditable.UpdatedAt));;
                    if (!updatedAtProperty.IsModified ||
                        (DateTimeOffset)updatedAtProperty.CurrentValue! < DateTimeOffset.UtcNow.AddSeconds(-5))
                    {
                        updatedAtProperty.CurrentValue = DateTimeOffset.UtcNow;
                    }
                }

                if (entry.State == EntityState.Added)
                {
                    var createdAtProperty = entry.Property(nameof(IAuditable.CreatedAt));;
                    createdAtProperty.CurrentValue ??= DateTimeOffset.UtcNow;
                }

            }
        }

        _logger.LogDebug("Added audit information to {EntryCount} entities", entries.Count);
    }



    private void ApplyGlobalFilters(ModelBuilder modelBuilder)
    {
        // Global query filter for tenant isolation
        //modelBuilder.Entity<Job>().HasQueryFilter(j => j.TenantId == _tenantService.GetCurrentTenant());

        _logger.LogDebug("Global filters applied (hook, none configured)");
    }

    private void ConfigureConventions(ModelBuilder modelBuilder)
    {
        _logger.LogDebug("Add default string length (255) convention)");
        // Configure string properties to have reasonable defaults
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(string))
                {
                    // Set default max length for string properties
                    if (property.GetMaxLength() == null)
                    {
                        property.SetMaxLength(255);
                    }
                }
            }
        }

        // Configure DateTime properties for PostgreSQL
        _logger.LogDebug("Add postgres column type convention for DateTime properties");
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("timestamp");
                }
            }
        }
    }

}
