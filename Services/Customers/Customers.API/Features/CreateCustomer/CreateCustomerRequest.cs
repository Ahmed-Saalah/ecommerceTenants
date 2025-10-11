using MediatR;

namespace Customers.API.Features.CreateCustomer;

public sealed record CreateCustomerRequest(
    int CustomerId,
    int TenantId,
    string Username,
    string? Email,
    string? PhoneNumber,
    string DisplayName
) : IRequest<CreateCustomerResponse>;
