using System.ComponentModel.DataAnnotations;

namespace NotificationService.API.Application.DTOs;

public class SendMassiveRequest
{
    [Required(ErrorMessage = "El ID del evento es obligatorio")]
    public int EventoId { get; set; }

    [Required(ErrorMessage = "El tipo de notificación es obligatorio")]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los datos del evento son obligatorios")]
    public EventDto DatosEvento { get; set; } = null!;
    
    [Required(ErrorMessage = "La lista de participantes es obligatoria")]
    [MinLength(1, ErrorMessage = "Debe incluir al menos un participante")]
    public List<ParticipantDto> Participantes { get; set; } = [];
}