using NotificationService.API.Domain.Enums;

namespace NotificationService.API.Application.Mapping;

public static class NotificationTypeMapper
{
    private static readonly Dictionary<string, NotificationType> StringToEnum = new(StringComparer.OrdinalIgnoreCase)
    {
        { "CONFIRMACION", NotificationType.RegistrationConfirmation },
        { "CANCELACION", NotificationType.RegistrationCancellation },
        { "ACTUALIZACION", NotificationType.EventUpdate },
        { "RECORDATORIO", NotificationType.EventReminder },
        { "CANCELACION_EVENTO", NotificationType.EventCancellation }
    };

    private static readonly Dictionary<NotificationType, string> EnumToString = new()
    {
        { NotificationType.RegistrationConfirmation, "CONFIRMACION" },
        { NotificationType.RegistrationCancellation, "CANCELACION" },
        { NotificationType.EventUpdate, "ACTUALIZACION" },
        { NotificationType.EventReminder, "RECORDATORIO" },
        { NotificationType.EventCancellation, "CANCELACION_EVENTO" }
    };

    public static NotificationType ToEnum(string tipo)
    {
        return StringToEnum.TryGetValue(tipo, out var result) ? 
            result : 
            throw new ArgumentException($"Tipo de notificación inválido: {tipo}. Valores válidos: CONFIRMACION, CANCELACION, ACTUALIZACION, RECORDATORIO, CANCELACION_EVENTO");
    }

    public static string ToString(NotificationType tipo)
    {
        return EnumToString.TryGetValue(tipo, out var result) ? 
            result : 
            throw new ArgumentException($"Tipo de notificación desconocido: {tipo}");
    }

    public static bool IsValid(string tipo)
    {
        return StringToEnum.ContainsKey(tipo);
    }
}