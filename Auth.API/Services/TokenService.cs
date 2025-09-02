using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Auth.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Auth.API.Services;

public sealed class TokenService : ITokenService
{
    private readonly SymmetricSecurityKey _key;
    private readonly UserManager<User> _userManager;

    public TokenService(IConfiguration config, UserManager<User> userManager)
    {
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
        _userManager = userManager;
    }

    public async Task<(string, DateTime)> CreateTokenAsync(User user, int[] tenantIds)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? ""),
            new(ClaimTypes.GivenName, user.DisplayName ?? ""),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(ClaimTypes.GroupSid, string.Join(",", tenantIds)),
        };

        var roles = await _userManager.GetRolesAsync(user);

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);

        var expiresIn = DateTime.UtcNow.AddHours(1);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresIn,
            SigningCredentials = creds,
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expiresIn);
    }

    public async Task<(string, DateTime)> CreateImpersonationTokenAsync(
        User impersonator,
        User impersonatedUser,
        string sessionId,
        int[] tenantIds
    )
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, impersonatedUser.Id.ToString()),
            new(ClaimTypes.Name, impersonatedUser.UserName ?? ""),
            new(ClaimTypes.GivenName, impersonatedUser.DisplayName ?? ""),
            new(ClaimTypes.Email, impersonatedUser.Email ?? ""),
            new(ClaimTypes.GroupSid, string.Join(",", tenantIds)),
            new("impersonated_by_id", impersonator?.Id.ToString() ?? ""),
            new("impersonated_by_name", impersonator?.UserName ?? ""),
            new("session_id", sessionId),
        };

        var roles = await _userManager.GetRolesAsync(impersonatedUser);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);
        var expiresIn = DateTime.UtcNow.AddHours(1);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresIn,
            SigningCredentials = creds,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expiresIn);
    }

    public RefreshToken GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return new RefreshToken { Token = Convert.ToBase64String(randomNumber) };
    }
}
