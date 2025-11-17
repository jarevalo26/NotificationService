namespace NotificationService.API.Application.Interfaces;

public interface ISendGridEmailService
{
    Task<(bool Success, string? MessageId, string? ErrorMessage)> SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlContent,
        string? textContent = null);

    Task<bool> ValidateConnectionAsync();
}