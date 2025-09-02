using Auth.API.Helpers;

namespace Auth.API.Mediator.CreateUser;

public class CreateUserResponse : Result<CreateUserResponseDto>
{
    public static implicit operator CreateUserResponse(CreateUserResponseDto successResult) =>
        new() { Value = successResult };

    public static implicit operator CreateUserResponse(DomainError errorResult) =>
        new() { Error = errorResult };
}

public sealed record CreateUserResponseDto(
    int UserId,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresIn
);
