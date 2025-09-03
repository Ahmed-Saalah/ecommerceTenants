using Core.DataAccess.Abstractions;
using Customers.API.Helpers;
using Customers.API.Models;
using FluentValidation;
using MediatR;

namespace Customers.API.Features.CreateCustomer;

public sealed class CreateCustomerHandler(
    IEntityWriter<Customer> customerWriter,
    IEntityWriter<CustomerTenant> customerTenantWriter,
    IValidator<CreateCustomerRequest> validator
) : IRequestHandler<CreateCustomerRequest, CreateCustomerResponse>
{
    public async Task<CreateCustomerResponse> Handle(
        CreateCustomerRequest request,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return new ValidationError(validationResult.Errors);
        }

        var customer = new Customer
        {
            CustomerId = request.CustomerId,
            Username = request.Username,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            DisplayName = request.DisplayName,
            Timestamp = DateTime.UtcNow,
            AddressId = null,
        };

        var customerTenant = new CustomerTenant
        {
            CustomerId = request.CustomerId,
            TenantId = request.TenantId,
            Timestamp = DateTime.UtcNow,
        };

        await customerWriter.UpsertAsync(customer, c => c.CustomerId);

        await customerTenantWriter.UpsertAsync(
            customerTenant,
            request.CustomerId,
            request.TenantId
        );

        return new ResponseDto(
            customer.CustomerId,
            customerTenant.TenantId,
            customer.Username,
            customer.Email,
            customer.PhoneNumber,
            customer.DisplayName
        );
    }
}
