using System.Net.Http;
using Customers.EventHandlers.Clients.Abstract;

namespace Customers.EventHandlers.Clients.Customers;

public interface ICustomersClient : IApiClient { }

public sealed class CustomersClient(HttpClient httpClient)
    : BaseApiClient(httpClient),
        ICustomersClient { }
