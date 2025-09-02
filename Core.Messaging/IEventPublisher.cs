namespace Core.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, string topic, CancellationToken cancellationToken = default);
}
