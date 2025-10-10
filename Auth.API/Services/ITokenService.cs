using Auth.API.Models;

namespace Auth.API.Services;

public interface ITokenService
{
    Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(
        User user,
        string ipAddress
    );
    string GenerateAccessToken(User user);
    string GenerateRefreshTokenString();
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(
        RefreshToken token,
        string ipAddress,
        string? replacedByToken = null
    );
    Task<(string AccessToken, string RefreshToken)> RefreshAsync(string token, string ipAddress);
}
