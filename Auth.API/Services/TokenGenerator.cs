using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Auth.API.Models;
using Microsoft.IdentityModel.Tokens;

namespace Auth.API.Services;

public interface ITokenGenerator
{
    // Generate with a single active tenant
    (string AccessToken, string RefreshToken, DateTime Expires) Generate(
        User user,
        int activeTenantId
    );

    // Generate with many memberships (and optional active tenant)
    (string AccessToken, string RefreshToken, DateTime Expires) Generate(
        User user,
        IEnumerable<int> tenantIds,
        int? activeTenantId = null
    );
}

public sealed class TokenGenerator : ITokenGenerator
{
    private readonly IConfiguration _config;

    public TokenGenerator(IConfiguration config)
    {
        _config = config;
    }

    public (string AccessToken, string RefreshToken, DateTime Expires) Generate(
        User user,
        int activeTenantId
    ) => Generate(user, new[] { activeTenantId }, activeTenantId);

    public (string AccessToken, string RefreshToken, DateTime Expires) Generate(
        User user,
        IEnumerable<int> tenantIds,
        int? activeTenantId = null
    )
    {
        var jwtSection = _config.GetSection("Jwt");
        var issuer = jwtSection["Issuer"] ?? string.Empty;
        var audience = jwtSection["Audience"] ?? string.Empty;
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");

        var accessMinutes = int.TryParse(jwtSection["AccessTokenMinutes"], out var m) ? m : 120;
        var accessExpires = DateTime.UtcNow.AddMinutes(accessMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new("displayName", user.DisplayName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(
                JwtRegisteredClaimNames.Iat,
                EpochTime.GetIntDate(DateTime.UtcNow).ToString(),
                ClaimValueTypes.Integer64
            ),
        };

        // Active tenant (the one this session is for)
        if (activeTenantId.HasValue)
            claims.Add(new Claim("tenantId", activeTenantId.Value.ToString()));

        // Membership list (optional, as a single claim)
        var tenantList = tenantIds?.Distinct().ToArray() ?? Array.Empty<int>();
        if (tenantList.Length > 0)
            claims.Add(new Claim("tenantIds", string.Join(",", tenantList)));

        // Roles (only if already loaded)
        if (user.UserRoles?.Any() == true)
        {
            foreach (var ur in user.UserRoles)
            {
                var roleName = ur.Role?.Name;
                if (!string.IsNullOrWhiteSpace(roleName))
                    claims.Add(new Claim(ClaimTypes.Role, roleName));
            }
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: accessExpires,
            signingCredentials: creds
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        var refreshToken = GenerateRefreshToken();

        return (accessToken, refreshToken, accessExpires);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        // Base64Url encode (no +, /, =)
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
