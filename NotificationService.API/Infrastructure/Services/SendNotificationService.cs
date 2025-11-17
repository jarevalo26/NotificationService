using Microsoft.EntityFrameworkCore;
using NotificationService.API.Application.DTOs;
using NotificationService.API.Application.Interfaces;
using NotificationService.API.Application.Mapping;
using NotificationService.API.Domain.Entities;
using NotificationService.API.Domain.Enums;
using NotificationService.API.Infrastructure.Persistences;

namespace NotificationService.API.Infrastructure.Services;

public class SendNotificationService : ISendNotificationService
{
    private readonly NotificationsDbContext _context;
    private readonly ISendGridEmailService _emailService;
    private readonly ILogger<SendNotificationService> _logger;
    
    public SendNotificationService(
        NotificationsDbContext context,
        ISendGridEmailService emailService,
        ILogger<SendNotificationService> logger
    )
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }
    
    public async Task<NotificationResponseDto> SendNotificationAsync(NotificationRequestDto request)
    {
        var notification = new Notification
        {
            RecipientEmail = request.RecipientEmail,
            RecipientName = request.RecipientName,
            Subject = request.Subject,
            HtmlBody = request.HtmlBody,
            TextBody = request.TextBody,
            NotificationType = request.NotificationType,
            Status = NotificationStatus.Pending,
            EventId = request.EventId,
            ParticipantId = request.ParticipantId,
            SentAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Notification created: {NotificationId} for {Email}",
            notification.Id,
            notification.RecipientEmail
        );
        
        notification.Status = NotificationStatus.Processing;
        notification.SendAttempts++;
        await _context.SaveChangesAsync();

        var (success, messageId, errorMessage) = await _emailService.SendEmailAsync(
            notification.RecipientEmail,
            notification.RecipientName,
            notification.Subject,
            notification.HtmlBody,
            notification.TextBody
        );

        if (success)
        {
            notification.Status = NotificationStatus.Sent;
            notification.CreatedAt = DateTime.UtcNow;
            notification.SendGridMessageId = messageId;
            notification.ErrorMessage = null;

            _logger.LogInformation(
                "Notification sent successfully: {NotificationId}",
                notification.Id
            );
        }
        else
        {
            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = errorMessage;

            _logger.LogError(
                "Failed to send notification: {NotificationId}. Error: {Error}",
                notification.Id,
                errorMessage
            );
        }

        await _context.SaveChangesAsync();

        return MapToResponseDto(notification);
    }

    public async Task<List<NotificationResponseDto>> SendMassiveNotificationsAsync(List<NotificationRequestDto> request)
    {
        var results = new List<NotificationResponseDto>();

        foreach (var notificationRequest in request)
        {
            var result = await SendNotificationAsync(notificationRequest);
            results.Add(result);
        }

        _logger.LogInformation(
            "Batch notifications processed: {Total} total, {Success} successful, {Failed} failed",
            results.Count,
            results.Count(r => r.Status == nameof(NotificationStatus.Sent)),
            results.Count(r => r.Status == nameof(NotificationStatus.Failed))
        );

        return results;
    }
    
    public async Task<NotificationResponseDto?> RetryNotificationAsync(Guid id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        
        if (notification == null)
        {
            _logger.LogWarning("Notification not found for retry: {NotificationId}", id);
            return null;
        }

        if (notification.SendAttempts >= 3)
        {
            _logger.LogWarning(
                "Notification has exceeded max retry attempts: {NotificationId}",
                id
            );
            return MapToResponseDto(notification);
        }

        _logger.LogInformation("Retrying notification: {NotificationId}", id);

        notification.Status = NotificationStatus.Retrying;
        notification.SendAttempts++;
        await _context.SaveChangesAsync();

        var (success, messageId, errorMessage) = await _emailService.SendEmailAsync(
            notification.RecipientEmail,
            notification.RecipientName,
            notification.Subject,
            notification.HtmlBody,
            notification.TextBody
        );

        if (success)
        {
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
            notification.SendGridMessageId = messageId;
            notification.ErrorMessage = null;

            _logger.LogInformation(
                "Notification retry successful: {NotificationId}",
                notification.Id
            );
        }
        else
        {
            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = errorMessage;

            _logger.LogError(
                "Notification retry failed: {NotificationId}. Error: {Error}",
                notification.Id,
                errorMessage
            );
        }

        await _context.SaveChangesAsync();
        return MapToResponseDto(notification);
    }
    
    private static NotificationResponseDto MapToResponseDto(Notification notification)
    {
        return new NotificationResponseDto
        {
            Id = notification.Id,
            ParticipanteEmail = notification.RecipientEmail,
            ParticipanteName = notification.RecipientName,
            Subject = notification.Subject,
            TipoNotificacion = NotificationTypeMapper.ToString(notification.NotificationType),
            Status = NotificationStatusMapper.ToString(notification.Status),
            FechaCreacion = notification.CreatedAt,
            FechaEnvio = notification.SentAt,
            Intentos = notification.SendAttempts,
            ErrorMessage = notification.ErrorMessage
        };
    }
}