using NotificationService.API.Application.DTOs;

namespace NotificationService.API.Application.Interfaces;

public interface IRabbitMqService
{
    void PublishNotification(NotificationRequestDto notification);
    void PublishMassiveNotifications(List<NotificationRequestDto> notifications);
}