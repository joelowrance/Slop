using VerdaVidaLawnCare.CoreAPI.Models;

namespace VerdaVidaLawnCare.CoreAPI.Features.Estimates;

/// <summary>
/// Repository interface for estimate-related data operations
/// </summary>
public interface IEstimateRepository
{
    /// <summary>
    /// Finds a customer by email address
    /// </summary>
    /// <param name="email">The customer's email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The customer if found, null otherwise</returns>
    Task<Customer?> FindCustomerByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a customer by phone number
    /// </summary>
    /// <param name="phone">The customer's phone number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The customer if found, null otherwise</returns>
    Task<Customer?> FindCustomerByPhoneAsync(string phone, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new customer to the database
    /// </summary>
    /// <param name="customer">The customer to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added customer with generated ID</returns>
    Task<Customer> AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a service by its ID
    /// </summary>
    /// <param name="serviceId">The service ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The service if found, null otherwise</returns>
    Task<Service?> GetServiceByIdAsync(int serviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets equipment by its ID
    /// </summary>
    /// <param name="equipmentId">The equipment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The equipment if found, null otherwise</returns>
    Task<Equipment?> GetEquipmentByIdAsync(int equipmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new estimate to the database
    /// </summary>
    /// <param name="estimate">The estimate to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added estimate with generated ID</returns>
    Task<Estimate> AddEstimateAsync(Estimate estimate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple line items to the database
    /// </summary>
    /// <param name="items">The line items to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task AddLineItemsAsync(IEnumerable<EstimateLineItem> items, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an estimate with all related details
    /// </summary>
    /// <param name="id">The estimate ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The estimate with customer and line items if found, null otherwise</returns>
    Task<Estimate?> GetEstimateWithDetailsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a database transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current database transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current database transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
