using Auth.API.Models;
using MediatR;

namespace Auth.API.Features.CreateUser
{
    public sealed record CreateUserRequest(
        string Username,
        string Email,
        string PhoneNumber,
        string Password,
        string DisplayName,
        string Role,
        string? AvatarPath,
        UserClaim[]? Claims
    ) : IRequest<CreateUserResponse>;
}
