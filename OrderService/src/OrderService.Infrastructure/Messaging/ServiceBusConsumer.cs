using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands;
using OrderService.Domain.Events;
using System.Text.Json;

namespace OrderService.Infrastructure.Messaging;

public class ServiceBusConsumer : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ServiceBusConsumer> _logger;
    private ServiceBusProcessor? _processor;

    public ServiceBusConsumer(
        ServiceBusClient client,
        IServiceScopeFactory scopeFactory,
        ILogger<ServiceBusConsumer> logger)
    {
        _client = client;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor = _client.CreateProcessor("stock-reserved", "order-service-sub", new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 5,
            AutoCompleteMessages = false
        });

        _processor.ProcessMessageAsync += OnMessageReceived;
        _processor.ProcessErrorAsync += OnError;

        await _processor.StartProcessingAsync(stoppingToken);
    }

    private async Task OnMessageReceived(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            var stockEvent = JsonSerializer.Deserialize<StockReservedEvent>(body);

            if (stockEvent is not null)
            {
                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<StockReservedEventHandler>();
                await handler.HandleAsync(stockEvent, args.CancellationToken);
            }

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing StockReservedEvent for message {MessageId}", args.Message.MessageId);
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task OnError(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus error on {EntityPath}", args.EntityPath);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null)
            await _processor.StopProcessingAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
