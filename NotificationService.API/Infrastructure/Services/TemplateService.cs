using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using NotificationService.API.Application.Interfaces;
using NotificationService.API.Domain.Entities;
using NotificationService.API.Domain.Enums;
using NotificationService.API.Infrastructure.Persistences;

namespace NotificationService.API.Infrastructure.Services;

public class TemplateService: ITemplateService
{
    private readonly NotificationsDbContext _context;

    public TemplateService(
        NotificationsDbContext context
    )
    {
        _context = context;
    }
    
    public async Task<Template?> GetByTypeAsync(NotificationType type)
    {
        return await _context.Templates
            .Where(t => t.NotificationType == type && t.IsActive)
            .FirstOrDefaultAsync();
    }
    
    public string RenderTemplate(string template, Dictionary<string, string> variables)
    {
        var result = template;

        foreach (var variable in variables)
        {
            var pattern = $@"{{{{{variable.Key}}}}}";
            result = Regex.Replace(result, pattern, variable.Value, RegexOptions.IgnoreCase);
        }

        return result;
    }
}