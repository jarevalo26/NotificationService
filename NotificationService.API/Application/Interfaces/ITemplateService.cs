using NotificationService.API.Domain.Entities;
using NotificationService.API.Domain.Enums;

namespace NotificationService.API.Application.Interfaces;

public interface ITemplateService
{
    Task<Template?> GetByTypeAsync(NotificationType type);
    string RenderTemplate(string template, Dictionary<string, string> variables);
}