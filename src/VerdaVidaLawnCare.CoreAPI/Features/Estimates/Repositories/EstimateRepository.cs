using Microsoft.EntityFrameworkCore;
using VerdaVidaLawnCare.CoreAPI.Data;
using VerdaVidaLawnCare.CoreAPI.Models;

namespace VerdaVidaLawnCare.CoreAPI.Features.Estimates;

/// <summary>
/// Repository implementation for estimate-related data operations
/// </summary>
public class EstimateRepository : IEstimateRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EstimateRepository> _logger;

    public EstimateRepository(ApplicationDbContext context, ILogger<EstimateRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Customer?> FindCustomerByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Finding customer by email: {Email}", email);
        
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == email && c.IsActive, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Customer?> FindCustomerByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Finding customer by phone: {Phone}", phone);
        
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Phone == phone && c.IsActive, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Customer> AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Adding new customer: {Email}", customer.Email);
        
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Customer added successfully with ID: {CustomerId}", customer.Id);
        return customer;
    }

    /// <inheritdoc />
    public async Task<Service?> GetServiceByIdAsync(int serviceId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting service by ID: {ServiceId}", serviceId);
        
        return await _context.Services
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.IsActive, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Equipment?> GetEquipmentByIdAsync(int equipmentId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting equipment by ID: {EquipmentId}", equipmentId);
        
        return await _context.Equipment
            .FirstOrDefaultAsync(e => e.Id == equipmentId && e.IsActive, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Estimate> AddEstimateAsync(Estimate estimate, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Adding new estimate for customer: {CustomerId}", estimate.CustomerId);
        
        _context.Estimates.Add(estimate);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Estimate added successfully with ID: {EstimateId}", estimate.Id);
        return estimate;
    }

    /// <inheritdoc />
    public async Task AddLineItemsAsync(IEnumerable<EstimateLineItem> items, CancellationToken cancellationToken = default)
    {
        var itemsList = items.ToList();
        _logger.LogDebug("Adding {Count} line items", itemsList.Count);
        
        _context.EstimateLineItems.AddRange(itemsList);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Line items added successfully");
    }

    /// <inheritdoc />
    public async Task<Estimate?> GetEstimateWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting estimate with details by ID: {EstimateId}", id);
        
        return await _context.Estimates
            .Include(e => e.Customer)
            .Include(e => e.EstimateLineItems)
                .ThenInclude(li => li.Service)
            .Include(e => e.EstimateLineItems)
                .ThenInclude(li => li.Equipment)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Beginning database transaction");
        await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Committing database transaction");
        await _context.Database.CommitTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Rolling back database transaction");
        await _context.Database.RollbackTransactionAsync(cancellationToken);
    }
}
