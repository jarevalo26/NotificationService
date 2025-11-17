using Microsoft.Extensions.Options;
using NotificationService.API.Application.Interfaces;
using NotificationService.API.Infrastructure.Configurations;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NotificationService.API.Infrastructure.Services;

public class SendGridEmailService: ISendGridEmailService
{
    private readonly SendGridSettings _settings;
    private readonly ILogger<SendGridEmailService> _logger;
    private readonly SendGridClient _client;

    public SendGridEmailService(
        IOptions<SendGridSettings> settings,
        ILogger<SendGridEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _client = new SendGridClient(_settings.ApiKey);
    }

    public async Task<(bool Success, string? MessageId, string? ErrorMessage)> SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlContent,
        string? textContent = null)
    {
        try
        {
            var from = new EmailAddress(_settings.FromEmail, _settings.FromName);
            var to = new EmailAddress(toEmail, toName);
            
            var msg = MailHelper.CreateSingleEmail(
                from,
                to,
                subject,
                textContent ?? subject,
                htmlContent
            );

            var response = await _client.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                var messageId = response.Headers.GetValues("X-Message-Id").FirstOrDefault();
                _logger.LogInformation(
                    "Email sent successfully to {Email}. MessageId: {MessageId}",
                    toEmail,
                    messageId
                );
                
                return (true, messageId, null);
            }
            else
            {
                var errorBody = await response.Body.ReadAsStringAsync();
                _logger.LogError(
                    "Failed to send email to {Email}. Status: {Status}. Error: {Error}",
                    toEmail,
                    response.StatusCode,
                    errorBody);
                
                return (false, null, $"SendGrid error: {response.StatusCode} - {errorBody}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while sending email to {Email}", toEmail);
            return (false, null, $"Exception: {ex.Message}");
        }
    }

    public async Task<bool> ValidateConnectionAsync()
    {
        try
        {
            var from = new EmailAddress(_settings.FromEmail, _settings.FromName);
            var to = new EmailAddress(_settings.FromEmail, "Test");
            var msg = MailHelper.CreateSingleEmail(from, to, "Test", "Test", "<p>Test</p>");
            msg.SetSandBoxMode(true);

            var response = await _client.SendEmailAsync(msg);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate SendGrid connection");
            return false;
        }
    }
}