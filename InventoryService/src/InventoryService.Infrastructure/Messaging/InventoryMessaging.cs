using Azure.Messaging.ServiceBus;
using InventoryService.Application.Handlers;
using InventoryService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace InventoryService.Infrastructure.Messaging;

public class AzureServiceBusPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;

    public AzureServiceBusPublisher(string connectionString) => _client = new ServiceBusClient(connectionString);

    public async Task PublishAsync<T>(T message, string topicName, CancellationToken cancellationToken = default) where T : class
    {
        await using var sender = _client.CreateSender(topicName);
        var json = JsonSerializer.Serialize(message);
        await sender.SendMessageAsync(new ServiceBusMessage(json) { ContentType = "application/json", Subject = typeof(T).Name }, cancellationToken);
    }

    public async ValueTask DisposeAsync() => await _client.DisposeAsync();
}

public class InventoryServiceBusConsumer : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InventoryServiceBusConsumer> _logger;
    private ServiceBusProcessor? _processor;

    public InventoryServiceBusConsumer(ServiceBusClient client, IServiceScopeFactory scopeFactory, ILogger<InventoryServiceBusConsumer> logger)
    {
        _client = client;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor = _client.CreateProcessor("order-created", "inventory-service-sub", new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 5,
            AutoCompleteMessages = false
        });
        _processor.ProcessMessageAsync += OnMessage;
        _processor.ProcessErrorAsync += OnError;
        await _processor.StartProcessingAsync(stoppingToken);
    }

    private async Task OnMessage(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            var orderEvent = JsonSerializer.Deserialize<OrderCreatedEventMessage>(body);
            if (orderEvent is not null)
            {
                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<OrderCreatedEventHandler>();
                await handler.HandleAsync(orderEvent, args.CancellationToken);
            }
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderCreatedEvent");
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task OnError(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus error");
        return Task.CompletedTask;
    }
}
