using NotificationService.API.Application.DTOs;

namespace NotificationService.API.Application.Interfaces;

public interface ISendNotificationService
{
    Task<NotificationResponseDto> SendNotificationAsync(NotificationRequestDto request);
    Task<List<NotificationResponseDto>> SendMassiveNotificationsAsync(List<NotificationRequestDto> request);
    Task<NotificationResponseDto?> RetryNotificationAsync(Guid id);
}