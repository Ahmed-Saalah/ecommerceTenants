using Auth.API.Helpers;
using MediatR;

namespace Auth.API.Mediator.Login;

public sealed record LoginRequest(
    string Username,
    string Password,
    string LoginMethod = "username-password"
) : IRequest<Result<Response>>;
