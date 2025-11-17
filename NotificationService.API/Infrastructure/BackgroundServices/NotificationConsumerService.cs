using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NotificationService.API.Application.DTOs;
using NotificationService.API.Application.Interfaces;
using NotificationService.API.Infrastructure.Configurations;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.API.Infrastructure.BackgroundServices;

public class NotificationConsumerService: BackgroundService
{
    private readonly RabbitMqSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationConsumerService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public NotificationConsumerService(
        IOptions<RabbitMqSettings> settings,
        IServiceProvider serviceProvider,
        ILogger<NotificationConsumerService> logger
    )
    {
        _settings = settings.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("NotificationConsumerService is starting");
        InitializeRabbitMq();
        return base.StartAsync(cancellationToken);
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
                Password = _settings.Password,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: _settings.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            _logger.LogInformation(
                "RabbitMQ consumer initialized. Queue: {QueueName}",
                _settings.QueueName
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ consumer");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        if (_channel == null)
        {
            _logger.LogError("RabbitMQ channel is null. Cannot start consumer.");
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation("Message received from RabbitMQ: {Message}", message);
                var notificationRequest = JsonSerializer.Deserialize<NotificationRequestDto>(message);

                if (notificationRequest != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var notificationService = scope.ServiceProvider
                        .GetRequiredService<ISendNotificationService>();

                    await notificationService.SendNotificationAsync(notificationRequest);
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    _logger.LogInformation("Notification processed successfully");
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize notification message");
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification from RabbitMQ");
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(
            queue: _settings.QueueName,
            autoAck: false,
            consumer: consumer
        );

        _logger.LogInformation("NotificationConsumerService is now listening for messages");
        await Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        _logger.LogInformation("NotificationConsumerService disposed");
        base.Dispose();
    }
}