using Auth.API.Helpers;
using Auth.API.Models;
using Auth.API.Services;
using Core.DataAccess.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.API.Features.CreateUser;

public sealed class CreateUserHandler(
    IEntityWriter<UserTenant> userTenantWriter,
    IValidator<CreateUserRequest> validator,
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    ITokenGenerator tokenGenerator
) : IRequestHandler<CreateUserRequest, CreateUserResponse>
{
    public async Task<CreateUserResponse> Handle(
        CreateUserRequest request,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return new ValidationError(validationResult.Errors);

        var user = new User
        {
            UserName = request.Username,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            DisplayName = request.DisplayName,
            AvatarPath = request.AvatarPath ?? "/images/default-avatar.png",
            RegisteredAt = DateTime.UtcNow,
        };

        var identityResult = await userManager.CreateAsync(user, request.Password);

        if (!identityResult.Succeeded)
            return new ValidationError(identityResult.Errors);

        var roleExists = await roleManager.RoleExistsAsync(request.Role);
        if (!roleExists)
            return new NotFound("Specified role does not exist");

        await userManager.AddToRoleAsync(user, request.Role);

        if (request.TenantIds is not null)
        {
            foreach (var tenantId in request.TenantIds.Distinct())
            {
                await userTenantWriter.AddAsync(
                    new UserTenant { UserId = user.Id, TenantId = tenantId }
                );
            }
        }

        if (request.Claims?.Any() == true)
        {
            await userManager.AddClaimsAsync(
                user,
                request.Claims.Select(c => new System.Security.Claims.Claim(c.Type, c.Value))
            );
        }

        var (accessToken, refreshToken, expiresAt) = tokenGenerator.Generate(
            user,
            request.TenantIds ?? Array.Empty<int>()
        );

        return new CreateUserResponseDto(user.Id, accessToken, refreshToken, expiresAt);
    }
}
