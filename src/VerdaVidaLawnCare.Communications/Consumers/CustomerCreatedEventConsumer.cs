using MassTransit;
using VerdaVida.Shared.Events;

namespace VerdaVidaLawnCare.Communications.Consumers;

/// <summary>
/// Consumer for CustomerCreatedEvent that logs customer creation details to console
/// </summary>
public class CustomerCreatedEventConsumer : IConsumer<CustomerCreatedEvent>
{
    private readonly ILogger<CustomerCreatedEventConsumer> _logger;

    public CustomerCreatedEventConsumer(ILogger<CustomerCreatedEventConsumer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Consumes the CustomerCreatedEvent and logs the details to console
    /// </summary>
    /// <param name="context">The consume context containing the event</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public Task Consume(ConsumeContext<CustomerCreatedEvent> context)
    {
        try
        {
            var message = context.Message;
            
            _logger.LogInformation(
                "🎉 NEW CUSTOMER CREATED! CustomerId: {CustomerId}, Email: {Email}, CreatedAt: {CreatedAt}",
                message.CustomerId,
                message.Email,
                message.CreatedAt);

            // Also write to console for immediate visibility
            Console.WriteLine($"🎉 NEW CUSTOMER CREATED!");
            Console.WriteLine($"   Customer ID: {message.CustomerId}");
            Console.WriteLine($"   Email: {message.Email}");
            Console.WriteLine($"   Created At: {message.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine($"   Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine("---");

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CustomerCreatedEvent for CustomerId: {CustomerId}", 
                context.Message.CustomerId);
            
            // Don't throw the exception to avoid message requeue loops
            return Task.CompletedTask;
        }
    }
}
