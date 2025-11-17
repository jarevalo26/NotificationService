using Microsoft.EntityFrameworkCore;
using NotificationService.API.Domain.Entities;
using NotificationService.API.Domain.Enums;

namespace NotificationService.API.Infrastructure.Persistences;

public static class DbInitializer
{
    public static void Initialize(NotificationsDbContext context)
    {
        if (context.Templates.Any())
        {
            return; 
        }

        var templates = new[]
        {
            new Template
            {
                Name = "Confirmacion Registro",
                NotificationType = NotificationType.RegistrationConfirmation,
                SubjectTemplate = "Confirmación de registro - {{EventName}}",
                HtmlBodyTemplate = """
                                   <html>
                                   <body style='font-family: Arial, sans-serif;'>
                                       <h2 style='color: #4CAF50;'>¡Registro Confirmado!</h2>
                                       <p>Hola <strong>{{ParticipantName}}</strong>,</p>
                                       <p>Tu registro para el evento <strong>{{EventName}}</strong> ha sido confirmado exitosamente.</p>
                                       <h3>Detalles del Evento:</h3>
                                       <ul>
                                           <li><strong>Fecha:</strong> {{EventDate}}</li>
                                           <li><strong>Hora:</strong> {{EventTime}}</li>
                                           <li><strong>Lugar:</strong> {{EventLocation}}</li>
                                       </ul>
                                       <p>¡Te esperamos!</p>
                                       <p style='color: #666; font-size: 12px;'>Este es un mensaje automático, por favor no responder.</p>
                                   </body>
                                   </html>
                                   """,
                TextBodyTemplate = "Hola {{ParticipantName}}, tu registro para {{EventName}} ha sido confirmado. Fecha: {{EventDate}}.",
                Description = "Plantilla para confirmar el registro de un participante a un evento",
                IsActive = true
            },
            new Template()
            {
                Name = "Cancelacion Registro",
                NotificationType = NotificationType.RegistrationCancellation,
                SubjectTemplate = "Cancelación de registro - {{EventName}}",
                HtmlBodyTemplate = """
                                   <html>
                                   <body style='font-family: Arial, sans-serif;'>
                                       <h2 style='color: #FF9800;'>Registro Cancelado</h2>
                                       <p>Hola <strong>{{ParticipantName}}</strong>,</p>
                                       <p>Tu registro para el evento <strong>{{EventName}}</strong> ha sido cancelado exitosamente.</p>
                                       <p>Si deseas volver a registrarte, puedes hacerlo en cualquier momento mientras haya cupos disponibles.</p>
                                       <p>¡Esperamos verte en futuros eventos!</p>
                                       <p style='color: #666; font-size: 12px;'>Este es un mensaje automático, por favor no responder.</p>
                                   </body>
                                   </html>
                                   """,
                TextBodyTemplate = "Hola {{ParticipantName}}, tu registro para {{EventName}} ha sido cancelado exitosamente.",
                Description = "Plantilla para confirmar la cancelación de registro de un participante",
                IsActive = true
            },
            new Template()
            {
                Name = "Actualizacion Evento",
                NotificationType = NotificationType.EventUpdate,
                SubjectTemplate = "Actualización importante - {{EventName}}",
                HtmlBodyTemplate = """
                                   <html>
                                   <body style='font-family: Arial, sans-serif;'>
                                       <h2 style='color: #2196F3;'>Actualización del Evento</h2>
                                       <p>Hola <strong>{{ParticipantName}}</strong>,</p>
                                       <p>Te informamos que el evento <strong>{{EventName}}</strong> ha sido actualizado.</p>
                                       <h3>Cambios realizados:</h3>
                                       <p>{{UpdateDetails}}</p>
                                       <h3>Nuevos Detalles:</h3>
                                       <ul>
                                           <li><strong>Fecha:</strong> {{EventDate}}</li>
                                           <li><strong>Hora:</strong> {{EventTime}}</li>
                                           <li><strong>Lugar:</strong> {{EventLocation}}</li>
                                       </ul>
                                       <p>Por favor, toma nota de estos cambios.</p>
                                       <p style='color: #666; font-size: 12px;'>Este es un mensaje automático, por favor no responder.</p>
                                   </body>
                                   </html>
                                   """,
                TextBodyTemplate = "Hola {{ParticipantName}}, el evento {{EventName}} ha sido actualizado. Detalles: {{UpdateDetails}}",
                Description = "Plantilla para notificar cambios en un evento",
                IsActive = true
            },
            new Template()
            {
                Name = "Recordatorio Evento",
                NotificationType = NotificationType.EventReminder,
                SubjectTemplate = "Recordatorio - {{EventName}} es mañana",
                HtmlBodyTemplate = """
                                   <html>
                                   <body style='font-family: Arial, sans-serif;'>
                                       <h2 style='color: #9C27B0;'>¡No lo olvides!</h2>
                                       <p>Hola <strong>{{ParticipantName}}</strong>,</p>
                                       <p>Te recordamos que el evento <strong>{{EventName}}</strong> se realizará pronto.</p>
                                       <h3>Detalles del Evento:</h3>
                                       <ul>
                                           <li><strong>Fecha:</strong> {{EventDate}}</li>
                                           <li><strong>Hora:</strong> {{EventTime}}</li>
                                           <li><strong>Lugar:</strong> {{EventLocation}}</li>
                                       </ul>
                                       <p>¡Te esperamos!</p>
                                       <p style='color: #666; font-size: 12px;'>Este es un mensaje automático, por favor no responder.</p>
                                   </body>
                                   </html>
                                   """,
                TextBodyTemplate = "Recordatorio: {{EventName}} será el {{EventDate}} a las {{EventTime}}. ¡No faltes!",
                Description = "Plantilla para recordar a los participantes sobre un evento próximo",
                IsActive = true
            },
            new Template()
            {
                Name = "Cancelacion Evento",
                NotificationType = NotificationType.EventCancellation,
                SubjectTemplate = "CANCELACIÓN - {{EventName}}",
                HtmlBodyTemplate = """
                                   <html>
                                   <body style='font-family: Arial, sans-serif;'>
                                       <h2 style='color: #F44336;'>Evento Cancelado</h2>
                                       <p>Hola <strong>{{ParticipantName}}</strong>,</p>
                                       <p>Lamentamos informarte que el evento <strong>{{EventName}}</strong> ha sido <strong>CANCELADO</strong>.</p>
                                       <p><strong>Motivo:</strong> {{CancellationReason}}</p>
                                       <p>Nos disculpamos por cualquier inconveniente que esto pueda causar.</p>
                                       <p>Te mantendremos informado sobre futuros eventos.</p>
                                       <p style='color: #666; font-size: 12px;'>Este es un mensaje automático, por favor no responder.</p>
                                   </body>
                                   </html>
                                   """,
                TextBodyTemplate = "El evento {{EventName}} ha sido cancelado. Motivo: {{CancellationReason}}. Disculpa las molestias.",
                Description = "Plantilla para notificar la cancelación de un evento",
                IsActive = true
            }
        };

        context.Templates.AddRange(templates);
        context.SaveChanges();
    }
}