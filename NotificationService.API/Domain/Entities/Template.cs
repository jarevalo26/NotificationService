using NotificationService.API.Domain.Enums;

namespace NotificationService.API.Domain.Entities;

public class Template
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public NotificationType NotificationType { get; set; }
    
    public string SubjectTemplate { get; set; } = string.Empty;
    
    public string HtmlBodyTemplate { get; set; } = string.Empty;

    public string? TextBodyTemplate { get; set; }
    
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}