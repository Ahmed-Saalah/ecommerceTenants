namespace Customers.EventHandlers.Clients.Abstract;

public interface IEndpoint
{
    string Url { get; }
    HttpMethod Method { get; }
}

public interface IActionEndpoint : IEndpoint { }

public interface IActionEndpoint<TRequest> : IEndpoint
{
    TRequest Request { get; }
}

public interface IQueryEndpoint<TResponse> : IEndpoint { }

public interface IQueryEndpoint<TRequest, TResponse> : IQueryEndpoint<TRequest>
{
    TRequest Request { get; }
}
