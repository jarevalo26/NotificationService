using NotificationService.API.Domain.Enums;

namespace NotificationService.API.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string RecipientEmail { get; set; } = string.Empty;
    
    public string RecipientName { get; set; } = string.Empty;
    
    public string Subject { get; set; } = string.Empty;
    
    public string HtmlBody { get; set; } = string.Empty;

    public string? TextBody { get; set; }
    
    public NotificationType NotificationType { get; set; }
    
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SentAt { get; set; }

    public int SendAttempts { get; set; } = 0;
    
    public string? ErrorMessage { get; set; }
    
    public int EventId { get; set; }

    public int ParticipantId { get; set; }
    
    public string? SendGridMessageId { get; set; }
}