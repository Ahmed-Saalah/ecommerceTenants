using Auth.API.Models;
using MediatR;

namespace Auth.API.Mediator.CreateGuest;

public sealed record CreateGuestRequest(
    string Username,
    string? Email,
    string PhoneNumber,
    string Password,
    string DisplayName,
    string Role,
    string? AvatarPath,
    int[]? TenantIds,
    UserClaim[]? Claims
) : IRequest<CreateGuestResponse>;
