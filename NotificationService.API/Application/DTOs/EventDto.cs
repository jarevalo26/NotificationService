using System.ComponentModel.DataAnnotations;

namespace NotificationService.API.Application.DTOs;

public class EventDto
{
    [Required(ErrorMessage = "El nombre del evento es obligatorio")]
    public string NombreEvento { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha del evento es obligatoria")]
    public DateTime FechaEvento { get; set; }

    [Required(ErrorMessage = "La ubicación del evento es obligatoria")]
    public string Ubicacion { get; set; } = string.Empty;

    public string? CambiosRealizados { get; set; }
}