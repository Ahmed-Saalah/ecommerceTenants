namespace Core.Messaging.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, string topic, CancellationToken cancellationToken = default);
}
