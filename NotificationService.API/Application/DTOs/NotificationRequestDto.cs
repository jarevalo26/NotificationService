using System.ComponentModel.DataAnnotations;
using NotificationService.API.Domain.Enums;

namespace NotificationService.API.Application.DTOs;

public class NotificationRequestDto
{
    [Required(ErrorMessage = "El email del destinatario es obligatorio")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
    public string RecipientEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del destinatario es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string RecipientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El asunto es obligatorio")]
    [StringLength(300, MinimumLength = 5, ErrorMessage = "El asunto debe tener entre 5 y 300 caracteres")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "El cuerpo del mensaje es obligatorio")]
    public string HtmlBody { get; set; } = string.Empty;

    public string? TextBody { get; set; }

    [Required(ErrorMessage = "El tipo de notificación es obligatorio")]
    public NotificationType NotificationType { get; set; }
    
    public int EventId { get; set; }
    public int ParticipantId { get; set; }
}