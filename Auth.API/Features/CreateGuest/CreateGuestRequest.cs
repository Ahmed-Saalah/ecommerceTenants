using Auth.API.Models;
using MediatR;

namespace Auth.API.Features.CreateGuest;

public sealed record CreateGuestRequest(
    string Username,
    string? Email,
    string PhoneNumber,
    string Password,
    string DisplayName,
    string Role,
    string? AvatarPath,
    UserClaim[]? Claims
) : IRequest<CreateGuestResponse>;
