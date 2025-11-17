using NotificationService.API.Domain.Enums;

namespace NotificationService.API.Application.Mapping;

public static class NotificationStatusMapper
{
    private static readonly Dictionary<string, NotificationStatus> StringToEnum = new(StringComparer.OrdinalIgnoreCase)
    {
        { "PENDIENTE", NotificationStatus.Pending},
        { "PROCESANDO", NotificationStatus.Processing },
        { "ENVIADA", NotificationStatus.Sent },
        { "FALLIDA", NotificationStatus.Failed },
        { "REINTENTANDO", NotificationStatus.Retrying }
    };

    private static readonly Dictionary<NotificationStatus, string> EnumToString = new()
    {
        { NotificationStatus.Pending, "PENDIENTE" },
        { NotificationStatus.Processing, "PROCESANDO" },
        { NotificationStatus.Sent, "ENVIADA" },
        { NotificationStatus.Failed, "FALLIDA" },
        { NotificationStatus.Retrying, "REINTENTANDO" }
    };

    public static NotificationStatus ToEnum(string tipo)
    {
        return StringToEnum.TryGetValue(tipo, out var result) ? 
            result : 
            throw new ArgumentException($"Tipo de notificación inválido: {tipo}. Valores válidos: CONFIRMACION, CANCELACION, ACTUALIZACION, RECORDATORIO, CANCELACION_EVENTO");
    }

    public static string ToString(NotificationStatus tipo)
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