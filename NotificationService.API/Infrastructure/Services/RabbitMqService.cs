using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NotificationService.API.Application.DTOs;
using NotificationService.API.Application.Interfaces;
using NotificationService.API.Infrastructure.Configurations;
using RabbitMQ.Client;

namespace NotificationService.API.Infrastructure.Services;

public class RabbitMqService: IRabbitMqService, IDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqService(
        IOptions<RabbitMqSettings> settings,
        ILogger<RabbitMqService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        InitializeRabbitMq();
    }

    private void InitializeRabbitMq()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: _settings.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _logger.LogInformation(
                "RabbitMQ connection established. Queue: {QueueName}",
                _settings.QueueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
        }
    }

    public void PublishNotification(NotificationRequestDto notification)
    {
        try
        {
            if (_channel == null || !_channel.IsOpen)
            {
                _logger.LogWarning("RabbitMQ channel is not open. Attempting to reconnect...");
                InitializeRabbitMq();
            }

            var message = JsonSerializer.Serialize(notification);
            var body = Encoding.UTF8.GetBytes(message);
            var properties = _channel!.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";

            _channel.BasicPublish(
                exchange: string.Empty,
                routingKey: _settings.QueueName,
                basicProperties: properties,
                body: body
            );

            _logger.LogInformation(
                "Notification published to RabbitMQ. Email: {Email}",
                notification.RecipientEmail
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish notification to RabbitMQ");
            throw;
        }
    }

    public void PublishMassiveNotifications(List<NotificationRequestDto> notifications)
    {
        try
        {
            if (_channel == null || !_channel.IsOpen)
            {
                _logger.LogWarning("RabbitMQ channel is not open. Attempting to reconnect...");
                InitializeRabbitMq();
            }

            var properties = _channel!.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";

            foreach (var body in notifications
                .Select(notification => JsonSerializer.Serialize(notification))
                .Select(message => Encoding.UTF8.GetBytes(message))
            )
            {
                _channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: _settings.QueueName,
                    basicProperties: properties,
                    body: body
                );
            }

            _logger.LogInformation(
                "Batch notifications published to RabbitMQ. Count: {Count}",
                notifications.Count
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish batch notifications to RabbitMQ");
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        _logger.LogInformation("RabbitMQ connection disposed");
    }
}