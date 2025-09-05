using System.Net;
using System.Net.Http.Json;

namespace Customers.EventHandlers.Clients.Abstract;

public interface IApiClient
{
    Task ActAsync(IActionEndpoint endpoint);
    Task ActAsync<TRequest>(IActionEndpoint<TRequest> endpoint);
    Task<TResponse> RequestAsync<TResponse>(IQueryEndpoint<TResponse> endpoint);
    Task<TResponse?> RequestAsync<TRequest, TResponse>(
        IQueryEndpoint<TRequest, TResponse> endpoint
    );
}

public class BaseApiClient(HttpClient httpClient) : IApiClient
{
    public async Task ActAsync(IActionEndpoint endpoint)
    {
        var requestMessage = new HttpRequestMessage(endpoint.Method, endpoint.Url);
        var responseMessage = await httpClient.SendAsync(requestMessage);
        if (!responseMessage.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Unexpected {responseMessage.StatusCode} status code from exports api: {endpoint.Method.Method} {endpoint.Url}"
            );
        }
    }

    public async Task ActAsync<TRequest>(IActionEndpoint<TRequest> endpoint)
    {
        var requestMessage = new HttpRequestMessage(endpoint.Method, endpoint.Url)
        {
            Content = JsonContent.Create(endpoint.Request),
        };
        var responseMessage = await httpClient.SendAsync(requestMessage);
        if (!responseMessage.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Unexpected {responseMessage.StatusCode} status code from exports api: {endpoint.Method.Method} {endpoint.Url}"
            );
        }
    }

    public async Task<TResponse> RequestAsync<TResponse>(IQueryEndpoint<TResponse> endpoint)
    {
        var requestMessage = new HttpRequestMessage(endpoint.Method, endpoint.Url);
        var responseMessage = await httpClient.SendAsync(requestMessage);
        if (!responseMessage.IsSuccessStatusCode)
        {
            throw new Exception(
                $"unexpected {responseMessage.StatusCode} status code from exports api: {endpoint.Method.Method} {endpoint.Url}"
            );
        }

        return responseMessage.StatusCode == HttpStatusCode.NoContent
            ? default
            : await responseMessage.Content.ReadFromJsonAsync<TResponse>();
    }

    public async Task<TResponse?> RequestAsync<TRequest, TResponse>(
        IQueryEndpoint<TRequest, TResponse> endpoint
    )
    {
        var requestMessage = new HttpRequestMessage(endpoint.Method, endpoint.Url)
        {
            Content = JsonContent.Create(endpoint.Request),
        };

        var responseMessage = await httpClient.SendAsync(requestMessage);
        if (!responseMessage.IsSuccessStatusCode)
        {
            throw new Exception(
                $"unexpected {responseMessage.StatusCode} status code from exports api: {endpoint.Method.Method} {endpoint.Url}"
            );
        }

        return responseMessage.StatusCode == HttpStatusCode.NoContent
            ? default
            : await responseMessage.Content.ReadFromJsonAsync<TResponse>();
    }
}
