namespace Customers.EventHandlers;

public interface IEventHandler
{
    bool CanHandle(string eventType);
    Task HandleAsync(string json);
}
