namespace OrderService.Application.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, string topicName, CancellationToken cancellationToken = default) where T : class;
}
