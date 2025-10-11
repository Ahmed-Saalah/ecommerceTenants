using Auth.API.Helpers;

namespace Auth.API.Features.CreateGuest;

public class CreateGuestResponse : Result<ResponseDto>
{
    public static implicit operator CreateGuestResponse(ResponseDto successResult) =>
        new() { Value = successResult };

    public static implicit operator CreateGuestResponse(DomainError errorResult) =>
        new() { Error = errorResult };
}

public sealed record ResponseDto(int UserId, string AccessToken, string RefreshToken);
