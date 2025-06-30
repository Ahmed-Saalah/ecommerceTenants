using Auth.API.DbContexts;
using Auth.API.Helpers;
using Auth.API.Mediator;
using Auth.API.Models.Constants;
using Auth.API.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Routes;

public static class UserApiRoutes
{
    public static IEndpointRouteBuilder MapUsersApi(this IEndpointRouteBuilder app)
    {
        var usersRouter = app.MapGroup("/api/users").WithTags("Users");

        usersRouter.MapPost(
            "",
            async ([FromBody] CreateUser.Request data, IMediator mediator) =>
            {
                var response = await mediator.Send(data);
                return response.ToHttpResult();
            }
        );

        usersRouter.MapPost(
            "customers/guest",
            async (IMediator mediator, [FromQuery] int[]? tenantIds) =>
            {
                var response = await mediator.Send(
                    new CreateGuest.Request(
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
        );

        app.MapPost(
            "/api/token/refresh",
            async ([FromBody] string refreshToken, AuthDbContext db, ITokenGenerator generator) =>
            {
                var token = await db
                    .RefreshTokens.Include(t => t.User)
                    .FirstOrDefaultAsync(t =>
                        t.Token == refreshToken && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow
                    );

                if (token == null)
                    return Results.Unauthorized();

                var (accessToken, newRefreshToken, expires) = generator.Generate(token.User);

                token.IsRevoked = true;
                await db.SaveChangesAsync();

                return Results.Ok(
                    new
                    {
                        accessToken,
                        refreshToken = newRefreshToken,
                        expires,
                    }
                );
            }
        );

        return app;
    }
}
