using Azure.Messaging.ServiceBus;
using OrderService.Application.Interfaces;
using System.Text.Json;

namespace OrderService.Infrastructure.Messaging;

public class AzureServiceBusPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;

    public AzureServiceBusPublisher(string connectionString)
    {
        _client = new ServiceBusClient(connectionString);
    }

    public async Task PublishAsync<T>(T message, string topicName, CancellationToken cancellationToken = default) where T : class
    {
        await using var sender = _client.CreateSender(topicName);
        var json = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(json)
        {
            ContentType = "application/json",
            Subject = typeof(T).Name
        };
        await sender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }

    public async ValueTask DisposeAsync() => await _client.DisposeAsync();
}
