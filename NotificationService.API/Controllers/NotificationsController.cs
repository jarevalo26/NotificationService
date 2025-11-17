using Microsoft.AspNetCore.Mvc;
using NotificationService.API.Application.DTOs;
using NotificationService.API.Application.Interfaces;
using NotificationService.API.Application.Mapping;
using NotificationService.API.Domain.Enums;

namespace NotificationService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly ISendNotificationService _sendNotificationService;
        private readonly ITemplateService _templateService;
        private readonly ILogger<NotificationsController> _logger;
        
        public NotificationsController(
            ISendNotificationService sendNotificationService,
            ITemplateService templateService,
            ILogger<NotificationsController> logger
        )
        {
            _sendNotificationService = sendNotificationService;
            _templateService = templateService;
            _logger = logger;
        }
        
        /// <summary>
        /// Enviar notificación simplificada (endpoint para integración con EventsAPI Java)
        /// </summary>
        [HttpPost("send-individual")]
        public async Task<ActionResult> SendIndividualNotification(
            [FromBody] SendRequest request)
        {
            try
            {
                if (!NotificationTypeMapper.IsValid(request.Tipo))
                {
                    return BadRequest(new
                    {
                        error = $"Tipo de notificación inválido: {request.Tipo}",
                        validTypes = new[] { "CONFIRMACION", "CANCELACION", "ACTUALIZACION", "RECORDATORIO", "CANCELACION_EVENTO" }
                    });
                }
                
                var tipoNotificacion = NotificationTypeMapper.ToEnum(request.Tipo);
                
                var template = await _templateService.GetByTypeAsync(tipoNotificacion);
                if (template == null)
                {
                    _logger.LogError("No se encontró plantilla para tipo: {Tipo}", request.Tipo);
                    return StatusCode(500, new
                    {
                        error = $"No existe plantilla configurada para el tipo: {request.Tipo}"
                    });
                }
                
                var participanteName = string.IsNullOrWhiteSpace(request.ParticipanteName)
                    ? "Estimado participante"
                    : request.ParticipanteName;

                var variables = new Dictionary<string, string>
                {
                    ["ParticipantName"] = participanteName,
                    ["EventName"] = request.DatosEvento.NombreEvento,
                    ["EventDate"] = request.DatosEvento.FechaEvento.ToString("dd/MM/yyyy"),
                    ["EventTime"] = request.DatosEvento.FechaEvento.ToString("HH:mm"),
                    ["EventLocation"] = request.DatosEvento.Ubicacion,
                    ["EventDateTime"] = request.DatosEvento.FechaEvento.ToString("dd/MM/yyyy HH:mm")
                };
                
                var asunto = _templateService.RenderTemplate(template.SubjectTemplate, variables);
                var cuerpoHtml = _templateService.RenderTemplate(template.HtmlBodyTemplate, variables);
                var cuerpoTexto = !string.IsNullOrEmpty(template.TextBodyTemplate)
                    ? _templateService.RenderTemplate(template.TextBodyTemplate, variables)
                    : null;
                
                var notificationRequest = new NotificationRequestDto
                {
                    RecipientEmail = request.ParticipanteEmail,
                    RecipientName = participanteName,
                    Subject = asunto,
                    HtmlBody = cuerpoHtml,
                    TextBody = cuerpoTexto,
                    NotificationType = tipoNotificacion,
                    EventId = request.EventoId
                };
                
                var result = await _sendNotificationService.SendNotificationAsync(notificationRequest);
                if (result.Status == NotificationStatusMapper.ToString(NotificationStatus.Sent))
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        notificationId = result.Id,
                        error = result.ErrorMessage ?? "Error desconocido al enviar la notificación"
                    });
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validación fallida en send-individual");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en send-individual");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }
        
        /// <summary>
        /// Enviar notificaciones masivas (múltiples participantes del mismo evento)
        /// </summary>
        [HttpPost("send-massive")]
        public async Task<ActionResult> SendMassiveNotifications(
            [FromBody] SendMassiveRequest request)
        {
            try
            {
                if (!NotificationTypeMapper.IsValid(request.Tipo))
                {
                    return BadRequest(new
                    {
                        error = $"Tipo de notificación inválido: {request.Tipo}",
                        validTypes = new[] { "CONFIRMACION", "CANCELACION", "ACTUALIZACION", "RECORDATORIO", "CANCELACION_EVENTO" }
                    });
                }
                
                var notificationType = NotificationTypeMapper.ToEnum(request.Tipo);
                
                var plantilla = await _templateService.GetByTypeAsync(notificationType);
                if (plantilla == null)
                {
                    _logger.LogError("No se encontró plantilla para tipo: {Tipo}", request.Tipo);
                    return StatusCode(500, new
                    {
                        error = $"No existe plantilla configurada para el tipo: {request.Tipo}"
                    });
                }
                
                var notifications = (
                    from participant in request.Participantes
                    let participantName = string.IsNullOrWhiteSpace(participant.ParticipanteNombre)
                        ? "Estimado participante"
                        : participant.ParticipanteNombre
                    let variables = new Dictionary<string, string>
                    {
                        ["ParticipantName"] = participantName,
                        ["EventName"] = request.DatosEvento.NombreEvento,
                        ["EventDate"] = request.DatosEvento.FechaEvento.ToString("dd/MM/yyyy"),
                        ["EventTime"] = request.DatosEvento.FechaEvento.ToString("HH:mm"),
                        ["EventLocation"] = request.DatosEvento.Ubicacion,
                        ["EventDateTime"] = request.DatosEvento.FechaEvento.ToString("dd/MM/yyyy HH:mm"),
                        ["UpdateDetails"] = request.DatosEvento.CambiosRealizados!,
                        ["CancellationReason"] = request.DatosEvento.CambiosRealizados!
                    }
                    let subject = _templateService.RenderTemplate(plantilla.SubjectTemplate, variables)
                    let htmlBody = _templateService.RenderTemplate(plantilla.HtmlBodyTemplate, variables)
                    let textBody = !string.IsNullOrEmpty(plantilla.TextBodyTemplate)
                        ? _templateService.RenderTemplate(plantilla.TextBodyTemplate, variables)
                        : null
                    select new NotificationRequestDto
                    {
                        RecipientEmail = participant.ParticipanteEmail,
                        RecipientName = participantName,
                        Subject = subject,
                        HtmlBody = htmlBody,
                        TextBody = textBody,
                        NotificationType = notificationType,
                        EventId = request.EventoId
                    }).ToList();

                var results = await _sendNotificationService
                    .SendMassiveNotificationsAsync(notifications);
                
                var totalSent = results.Count(r => r.Status == NotificationStatusMapper.ToString(NotificationStatus.Sent));
                var totalFailed = results.Count(r => r.Status == NotificationStatusMapper.ToString(NotificationStatus.Failed));

                _logger.LogInformation(
                    "Envío masivo completado: {Total} total, {Enviadas} enviadas, {Fallidas} fallidas",
                    results.Count,
                    totalSent,
                    totalFailed);
                
                return Ok(new
                {
                    success = true,
                    totalNotificaciones = results.Count,
                    notificacionesEnviadas = totalSent,
                    notificacionesFallidas = totalFailed,
                    detalles = results.Select(r => new
                    {
                        notificationId = r.Id,
                        email = r.ParticipanteEmail,
                        status = r.Status.ToString(),
                        errorMessage = r.ErrorMessage
                    })
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validación fallida en send-massive");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en send-massive");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }        
    }
}
