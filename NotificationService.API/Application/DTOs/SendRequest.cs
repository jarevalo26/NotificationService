using System.ComponentModel.DataAnnotations;

namespace NotificationService.API.Application.DTOs;

public class SendRequest
{
    [Required(ErrorMessage = "El ID del evento es obligatorio")]
    public int EventoId { get; set; }

    [Required(ErrorMessage = "El email del participante es obligatorio")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
    public string ParticipanteEmail { get; set; } = string.Empty;

    public string? ParticipanteName { get; set; }
    
    [Required(ErrorMessage = "El tipo de notificación es obligatorio")]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los datos del evento son obligatorios")]
    public EventDto DatosEvento { get; set; } = null!;
}