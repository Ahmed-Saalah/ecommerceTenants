using Customers.EventHandlers.Clients.Abstract;

namespace Customers.EventHandlers.Clients.Customers.Endpoints;

public sealed class CreateCustomer
{
    public sealed record Endpoint(Request request) : IActionEndpoint
    {
        public string Url => "/api/customers";

        public HttpMethod Method => HttpMethod.Post;
    }

    public sealed record Request(
        int CustomerId,
        int TenantId,
        string Username,
        string? Email,
        string? PhoneNumber,
        string DisplayName
    );
}
