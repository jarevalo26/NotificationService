using System.ComponentModel.DataAnnotations;

namespace NotificationService.API.Application.DTOs;

public class NotificationsRequestDto
{
    [Required]
    [MinLength(1, ErrorMessage = "Debe incluirse al menos una notificación")]
    public List<NotificationRequestDto> Notifications { get; set; } = [];
}