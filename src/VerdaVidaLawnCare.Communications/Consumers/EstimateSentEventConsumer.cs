using MassTransit;
using VerdaVida.Shared.Events;
using VerdaVidaLawnCare.Communications.Services;

namespace VerdaVidaLawnCare.Communications.Consumers;

/// <summary>
/// Consumer for EstimateSentEvent that sends estimate emails to customers
/// </summary>
public class EstimateSentEventConsumer : IConsumer<EstimateSentEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILiquidTemplateService _templateService;
    private readonly ILogger<EstimateSentEventConsumer> _logger;

    public EstimateSentEventConsumer(
        IEmailService emailService,
        ILiquidTemplateService templateService,
        ILogger<EstimateSentEventConsumer> logger)
    {
        _emailService = emailService;
        _templateService = templateService;
        _logger = logger;
    }

    /// <summary>
    /// Consumes the EstimateSentEvent and sends an email to the customer
    /// </summary>
    /// <param name="context">The consume context containing the event</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task Consume(ConsumeContext<EstimateSentEvent> context)
    {
        try
        {
            var estimate = context.Message;

            _logger.LogInformation(
                "Processing EstimateSentEvent for EstimateId: {EstimateId}, EstimateNumber: {EstimateNumber}, CustomerEmail: {CustomerEmail}",
                estimate.EstimateId,
                estimate.EstimateNumber,
                estimate.CustomerEmail);

            // Render the email template
            var renderResult = await _templateService.RenderTemplateAsync("EstimateEmail", estimate);
            
            if (!renderResult.IsSuccess)
            {
                _logger.LogError(
                    "Failed to render email template for EstimateId: {EstimateId}. Error: {Error}",
                    estimate.EstimateId,
                    renderResult.Error);
                
                // Don't throw exception to avoid message requeue loops
                // The email sending failure is logged but doesn't prevent the estimate from being marked as sent
                return;
            }

            // Create email subject
            var subject = $"Your Estimate #{estimate.EstimateNumber} - VerdaVida Lawn Care";

            // Send the email
            var sendResult = await _emailService.SendEmailAsync(
                to: estimate.CustomerEmail,
                subject: subject,
                htmlBody: renderResult.Value);

            if (!sendResult.IsSuccess)
            {
                _logger.LogError(
                    "Failed to send estimate email for EstimateId: {EstimateId}, EstimateNumber: {EstimateNumber}, CustomerEmail: {CustomerEmail}. Error: {Error}",
                    estimate.EstimateId,
                    estimate.EstimateNumber,
                    estimate.CustomerEmail,
                    sendResult.Error);
            }
            else
            {
                _logger.LogInformation(
                    "Successfully sent estimate email for EstimateId: {EstimateId}, EstimateNumber: {EstimateNumber}, CustomerEmail: {CustomerEmail}",
                    estimate.EstimateId,
                    estimate.EstimateNumber,
                    estimate.CustomerEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error processing EstimateSentEvent for EstimateId: {EstimateId}",
                context.Message.EstimateId);
            
            // Don't throw the exception to avoid message requeue loops
            // The email sending failure is logged but doesn't prevent the estimate from being marked as sent
        }
    }
}

