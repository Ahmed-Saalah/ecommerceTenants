namespace Auth.API.Features.Login;

public sealed record Response(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresIn,
    string Role,
    ProfileData Profile
);

public sealed record ProfileData(
    string UserName,
    string Email,
    string DisplayName,
    string AvatarPath
);
