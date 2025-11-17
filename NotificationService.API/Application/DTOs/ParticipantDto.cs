using System.ComponentModel.DataAnnotations;

namespace NotificationService.API.Application.DTOs;

public class ParticipantDto
{
    [Required(ErrorMessage = "El email del participante es obligatorio")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
    public string ParticipanteEmail { get; set; } = string.Empty;

    public string? ParticipanteNombre { get; set; }
}