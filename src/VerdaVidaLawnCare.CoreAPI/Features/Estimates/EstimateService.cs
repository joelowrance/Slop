using Microsoft.EntityFrameworkCore;
using VerdaVida.Shared.Common;
using VerdaVida.Shared.Events;
using VerdaVida.Shared.OpenTelemetry;
using VerdaVidaLawnCare.CoreAPI.Data;
using VerdaVidaLawnCare.CoreAPI.Features.Estimates.DTOs;
using VerdaVidaLawnCare.CoreAPI.Models;
using MassTransit;

namespace VerdaVidaLawnCare.CoreAPI.Features.Estimates;

/// <summary>
/// Service for estimate operations
/// </summary>
public class EstimateService : IEstimateService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EstimateService> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly BusinessMetrics _businessMetrics;

    public EstimateService(
        ApplicationDbContext context, 
        ILogger<EstimateService> logger, 
        IPublishEndpoint publishEndpoint,
        BusinessMetrics businessMetrics)
    {
        _context = context;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _businessMetrics = businessMetrics;
    }

    /// <summary>
    /// Creates a new estimate with the provided information
    /// </summary>
    /// <param name="request">The estimate creation request</param>
    /// <returns>A result containing the created estimate details or an error message</returns>
    public async Task<Result<EstimateResponse>> CreateEstimateAsync(CreateEstimateRequest request)
    {
        try
        {
            _logger.LogInformation("Starting estimate creation for customer {CustomerEmail}", request.Customer.Email);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Find or create customer
                var customer = await FindOrCreateCustomerAsync(request.Customer);
                if (customer == null)
                {
                    _logger.LogWarning("Failed to find or create customer for email {Email}", request.Customer.Email);
                    return Result<EstimateResponse>.Failure("Failed to process customer information");
                }

                // Generate estimate number
                var estimateNumber = await GenerateEstimateNumberAsync();

                // Create estimate
                var estimate = new Estimate
                {
                    EstimateNumber = estimateNumber,
                    CustomerId = customer.Id,
                    EstimateDate = DateTime.UtcNow,
                    ExpirationDate = request.ExpirationDate?.ToUniversalTime() ?? DateTime.UtcNow.AddDays(30),
                    Status = EstimateStatus.Draft,
                    Notes = request.Notes,
                    Terms = request.Terms,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                _context.Estimates.Add(estimate);
                await _context.SaveChangesAsync();

                // Create line items
                var lineItems = new List<EstimateLineItem>();
                for (int i = 0; i < request.LineItems.Count; i++)
                {
                    var lineItemDto = request.LineItems[i];
                    var lineItem = new EstimateLineItem
                    {
                        EstimateId = estimate.Id,
                        ServiceId = lineItemDto.ServiceId,
                        EquipmentId = lineItemDto.EquipmentId,
                        Description = lineItemDto.Description,
                        Quantity = lineItemDto.Quantity,
                        UnitPrice = lineItemDto.UnitPrice,
                        LineTotal = lineItemDto.Quantity * lineItemDto.UnitPrice,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow
                    };
                    lineItems.Add(lineItem);
                }

                _context.EstimateLineItems.AddRange(lineItems);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Successfully created estimate {EstimateNumber} with {LineItemCount} line items",
                    estimateNumber, lineItems.Count);

                // Record metrics
                _businessMetrics.RecordEstimateReceived();
                var totalAmount = lineItems.Sum(li => li.LineTotal);
                _businessMetrics.RecordDollarValueBooked(totalAmount);

                // Return the created estimate
                await SendEstimateAsync(estimate.Id);
                return await GetEstimateResponseAsync(estimate.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating estimate for customer {CustomerEmail}", request.Customer.Email);
                throw;
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while creating estimate for customer {CustomerEmail}", request.Customer.Email);
            return Result<EstimateResponse>.Failure("A database error occurred while creating the estimate");
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while creating estimate for customer {CustomerEmail}: {Error}",
                request.Customer.Email, ex.Message);
            return Result<EstimateResponse>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating estimate for customer {CustomerEmail}", request.Customer.Email);
            return Result<EstimateResponse>.Failure("An unexpected error occurred while creating the estimate");
        }
    }

    /// <summary>
    /// Finds an existing customer or creates a new one
    /// </summary>
    private async Task<Customer?> FindOrCreateCustomerAsync(CustomerInfoDto customerInfo)
    {
        try
        {
            // Try to find existing customer by email
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == customerInfo.Email && c.IsActive);

            if (existingCustomer != null)
            {
                _logger.LogInformation("Found existing customer {CustomerId} for email {Email}",
                    existingCustomer.Id, customerInfo.Email);
                return existingCustomer;
            }

            // Create new customer
            var newCustomer = new Customer
            {
                FirstName = customerInfo.FirstName,
                LastName = customerInfo.LastName,
                Email = customerInfo.Email,
                Phone = customerInfo.Phone,
                Address = customerInfo.Address,
                City = customerInfo.City,
                State = customerInfo.State,
                PostalCode = customerInfo.PostalCode,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new customer {CustomerId} for email {Email}",
                newCustomer.Id, customerInfo.Email);

            // Publish customer created event
            try
            {
                var customerCreatedEvent = new CustomerCreatedEvent
                {
                    CustomerId = newCustomer.Id,
                    Email = newCustomer.Email,
                    CreatedAt = newCustomer.CreatedAt
                };

                await _publishEndpoint.Publish(customerCreatedEvent);
                _logger.LogInformation("Published CustomerCreatedEvent for customer {CustomerId}", newCustomer.Id);
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the customer creation
                _logger.LogWarning(ex, "Failed to publish CustomerCreatedEvent for customer {CustomerId}", newCustomer.Id);
            }

            return newCustomer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding or creating customer for email {Email}", customerInfo.Email);
            return null;
        }
    }

    /// <summary>
    /// Generates a unique estimate number
    /// </summary>
    private async Task<string> GenerateEstimateNumberAsync()
    {
        var today = DateTime.UtcNow;
        var datePrefix = today.ToString("yyyyMMdd");

        // Find the highest sequence number for today
        var existingEstimates = await _context.Estimates
            .Where(e => e.EstimateNumber.StartsWith($"EST-{datePrefix}-"))
            .Select(e => e.EstimateNumber)
            .ToListAsync();

        int sequence = 1;
        if (existingEstimates.Any())
        {
            var maxSequence = existingEstimates
                .Select(e => e.Split('-').Last())
                .Where(s => int.TryParse(s, out _))
                .Select(int.Parse)
                .DefaultIfEmpty(0)
                .Max();
            sequence = maxSequence + 1;
        }

        return $"EST-{datePrefix}-{sequence:D4}";
    }

    /// <summary>
    /// Gets the estimate response with all details
    /// </summary>
    private async Task<Result<EstimateResponse>> GetEstimateResponseAsync(int estimateId)
    {
        try
        {
            var estimate = await _context.Estimates
                .Include(e => e.Customer)
                .Include(e => e.EstimateLineItems)
                    .ThenInclude(eli => eli.Service)
                .Include(e => e.EstimateLineItems)
                    .ThenInclude(eli => eli.Equipment)
                .FirstOrDefaultAsync(e => e.Id == estimateId);

            if (estimate == null)
            {
                return Result<EstimateResponse>.Failure("Estimate not found");
            }

            var response = new EstimateResponse
            {
                Id = estimate.Id,
                EstimateNumber = estimate.EstimateNumber,
                Customer = new CustomerDto
                {
                    Id = estimate.Customer.Id,
                    FirstName = estimate.Customer.FirstName,
                    LastName= estimate.Customer.LastName,
                    Email = estimate.Customer.Email,
                    Phone = estimate.Customer.Phone,
                    FullAddress = $"{estimate.Customer.Address}, {estimate.Customer.City}, {estimate.Customer.State} {estimate.Customer.PostalCode}",
                    City = estimate.Customer.City,
                    State = estimate.Customer.State,
                    PostalCode = estimate.Customer.PostalCode
                },
                EstimateDate = estimate.EstimateDate,
                ExpirationDate = estimate.ExpirationDate,
                Status = estimate.Status.ToString(),
                Notes = estimate.Notes,
                Terms = estimate.Terms,
                LineItems = estimate.EstimateLineItems
                    .OrderBy(eli => eli.Id)
                    .Select(eli => new EstimateLineItemDto
                    {
                        ServiceId = eli.ServiceId,
                        EquipmentId = eli.EquipmentId,
                        Description = eli.Description,
                        Quantity = eli.Quantity,
                        UnitPrice = eli.UnitPrice,
                        LineTotal = eli.LineTotal,
                        ServiceName = eli.Service?.Name,
                        EquipmentName = eli.Equipment?.Name
                    })
                    .ToList(),
                CreatedAt = estimate.CreatedAt,
                UpdatedAt = estimate.UpdatedAt
            };

            // Calculate totals
            response.Subtotal = response.LineItems.Sum(li => li.LineTotal);
            response.TaxAmount = 0; // No tax for now
            response.TotalAmount = response.Subtotal + response.TaxAmount;

            // Calculate expiration info
            var daysUntilExpiration = (estimate.ExpirationDate - DateTime.UtcNow).Days;
            response.DaysUntilExpiration = Math.Max(0, daysUntilExpiration);
            response.IsExpired = daysUntilExpiration < 0;

            return Result<EstimateResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving estimate {EstimateId}", estimateId);
            return Result<EstimateResponse>.Failure("Error retrieving estimate details");
        }
    }

    /// <summary>
    /// Sends an estimate to the customer by updating its status to Sent and publishing an event
    /// </summary>
    public async Task<Result<EstimateResponse>> SendEstimateAsync(int estimateId)
    {
        try
        {
            _logger.LogInformation("Starting estimate send for EstimateId: {EstimateId}", estimateId);

            var estimate = await _context.Estimates
                .Include(e => e.Customer)
                .Include(e => e.EstimateLineItems)
                .FirstOrDefaultAsync(e => e.Id == estimateId);

            if (estimate == null)
            {
                _logger.LogWarning("Estimate not found: {EstimateId}", estimateId);
                return Result<EstimateResponse>.Failure("Estimate not found");
            }

            if (estimate.Status == EstimateStatus.Sent)
            {
                _logger.LogInformation("Estimate {EstimateId} is already sent", estimateId);
                return await GetEstimateResponseAsync(estimateId);
            }

            if (estimate.Status != EstimateStatus.Draft)
            {
                _logger.LogWarning("Cannot send estimate {EstimateId} with status {Status}", estimateId, estimate.Status);
                return Result<EstimateResponse>.Failure($"Cannot send estimate with status {estimate.Status}");
            }

            // Update status to Sent
            estimate.Status = EstimateStatus.Sent;
            estimate.UpdatedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated estimate {EstimateId} status to Sent", estimateId);

            // Publish EstimateSentEvent
            try
            {
                var lineItems = estimate.EstimateLineItems
                    .Select(li => new EstimateLineItemEvent
                    {
                        Description = li.Description,
                        Quantity = li.Quantity,
                        UnitPrice = li.UnitPrice,
                        LineTotal = li.LineTotal
                    })
                    .ToList();

                var subtotal = lineItems.Sum(li => li.LineTotal);
                var taxAmount = 0m; // No tax for now
                var totalAmount = subtotal + taxAmount;

                var estimateSentEvent = new EstimateSentEvent
                {
                    EstimateId = estimate.Id,
                    EstimateNumber = estimate.EstimateNumber,
                    CustomerId = estimate.CustomerId,
                    CustomerFirstName = estimate.Customer.FirstName,
                    CustomerLastName = estimate.Customer.LastName,
                    CustomerEmail = estimate.Customer.Email,
                    CustomerPostalCode = estimate.Customer.PostalCode,
                    EstimateDate = estimate.EstimateDate,
                    ExpirationDate = estimate.ExpirationDate,
                    Notes = estimate.Notes,
                    Terms = estimate.Terms,
                    LineItems = lineItems,
                    Subtotal = subtotal,
                    TaxAmount = taxAmount,
                    TotalAmount = totalAmount,
                    SentAt = DateTimeOffset.UtcNow
                };

                await _publishEndpoint.Publish(estimateSentEvent);
                _logger.LogInformation("Published EstimateSentEvent for estimate {EstimateId}", estimate.Id);
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the estimate send operation
                _logger.LogWarning(ex, "Failed to publish EstimateSentEvent for estimate {EstimateId}", estimate.Id);
            }

            return await GetEstimateResponseAsync(estimate.Id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while sending estimate {EstimateId}", estimateId);
            return Result<EstimateResponse>.Failure("A database error occurred while sending the estimate");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while sending estimate {EstimateId}", estimateId);
            return Result<EstimateResponse>.Failure("An unexpected error occurred while sending the estimate");
        }
    }
}
