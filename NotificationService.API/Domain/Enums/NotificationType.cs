namespace NotificationService.API.Domain.Enums;

public enum NotificationType
{
    RegistrationConfirmation = 1,
    RegistrationCancellation = 2,
    EventUpdate = 3,
    EventReminder = 4,
    EventCancellation = 5
}