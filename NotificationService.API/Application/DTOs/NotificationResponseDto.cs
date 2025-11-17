using NotificationService.API.Domain.Enums;

namespace NotificationService.API.Application.DTOs;

public class NotificationResponseDto
{
    public Guid Id { get; set; }
    public string ParticipanteEmail { get; set; } = string.Empty;
    public string ParticipanteName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? TipoNotificacion { get; set; } 
    public string Status { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaEnvio { get; set; }
    public int Intentos { get; set; }
    public string? ErrorMessage { get; set; }
}