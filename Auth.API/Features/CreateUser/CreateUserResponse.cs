using Auth.API.Helpers;

namespace Auth.API.Features.CreateUser;

public class CreateUserResponse : Result<CreateUserResponseDto>
{
    public static implicit operator CreateUserResponse(CreateUserResponseDto successResult) =>
        new() { Value = successResult };

    public static implicit operator CreateUserResponse(DomainError errorResult) =>
        new() { Error = errorResult };
}

public sealed record CreateUserResponseDto(int UserId, string AccessToken, string RefreshToke);
