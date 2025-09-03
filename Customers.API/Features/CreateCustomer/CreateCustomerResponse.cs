using Customers.API.Helpers;

namespace Customers.API.Features.CreateCustomer;

public sealed class CreateCustomerResponse : Result<ResponseDto>
{
    public static implicit operator CreateCustomerResponse(ResponseDto response) =>
        new() { Value = response };

    public static implicit operator CreateCustomerResponse(DomainError error) =>
        new() { Error = error };
}

public sealed record ResponseDto(
    int CustomerId,
    int TenantId,
    string Username,
    string? Email,
    string? PhoneNumber,
    string DisplayName
);
