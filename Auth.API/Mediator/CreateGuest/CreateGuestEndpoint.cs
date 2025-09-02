using Auth.API.Extensions;
using Auth.API.Helpers;
using Auth.API.Models.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Mediator.CreateGuest;

public sealed class CreateGuestEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/users/customers/guest",
                async (IMediator mediator, [FromQuery] int[]? tenantIds) =>
                {
                    var response = await mediator.Send(
                        new CreateGuestRequest(
                            Username: GuestData.Username,
                            Email: GuestData.Email,
                            PhoneNumber: GuestData.PhoneNumber,
                            Password: GuestData.Password,
                            DisplayName: GuestData.DisplayName,
                            Role: RoleConstants.Guest,
                            TenantIds: tenantIds,
                            Claims: [],
                            AvatarPath: null
                        )
                    );

                    return response.ToHttpResult();
                }
            )
            .WithTags("Users");
    }
}
