using Microsoft.EntityFrameworkCore;
using NotificationService.API.Application.Interfaces;
using NotificationService.API.Domain.Enums;
using NotificationService.API.Infrastructure.Persistences;

namespace NotificationService.API.Infrastructure.BackgroundServices;

public class RetryNotificationsService: BackgroundService
{
    private readonly TimeSpan _interval;
    private readonly int _maxAttempts;
    private readonly int _retryWindowHours;
    
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RetryNotificationsService> _logger;

    public RetryNotificationsService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<RetryNotificationsService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _interval = TimeSpan.FromMinutes(configuration.GetValue<int>("RetryService:IntervalMinutes", 5));
        _maxAttempts = configuration.GetValue<int>("RetryService:MaxRetryAttempts", 3);
        _retryWindowHours = configuration.GetValue<int>("RetryService:RetryWindowHours", 24);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RetryNotificationsService started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessFailedNotifications();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing failed notifications");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task ProcessFailedNotifications()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<ISendNotificationService>();
        
        var failedNotifications = await context.Notifications
            .Where(n => n.Status == NotificationStatus.Failed
                     && n.SendAttempts < _maxAttempts
                     && n.CreatedAt > DateTime.UtcNow.AddHours(-_retryWindowHours))
            .ToListAsync();

        if (failedNotifications.Count == 0)
        {
            _logger.LogInformation("No failed notifications to retry");
            return;
        }

        _logger.LogInformation(
            "Found {Count} failed notifications to retry", 
            failedNotifications.Count
        );

        foreach (var notification in failedNotifications)
        {
            try
            {
                await notificationService.RetryNotificationAsync(notification.Id);
                _logger.LogInformation(
                    "Retried notification {Id}", 
                    notification.Id
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, 
                    "Error retrying notification {Id}", 
                    notification.Id
                );
            }
        }
    }
}