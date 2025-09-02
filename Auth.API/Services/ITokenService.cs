using Auth.API.Models;

namespace Auth.API.Services;

public interface ITokenService
{
    Task<(string, DateTime)> CreateTokenAsync(User user, int[] tenantIds);
    Task<(string, DateTime)> CreateImpersonationTokenAsync(
        User impersonator,
        User impersonatedUser,
        string sessionId,
        int[] tenantIds
    );
    RefreshToken GenerateRefreshToken();
}
