using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Auth.API.DbContexts;
using Auth.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Auth.API.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly AuthDbContext _db;
    private readonly UserManager<User> _userManager;
    private readonly byte[] _key;
    private readonly int _accessMinutes;
    private readonly int _refreshDays;

    public TokenService(IConfiguration config, AuthDbContext db, UserManager<User> userManager)
    {
        _config = config;
        _db = db;
        _userManager = userManager;

        _key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
        _accessMinutes = int.Parse(_config["Jwt:AccessTokenMinutes"] ?? "120");
        _refreshDays = int.Parse(_config["Jwt:RefreshTokenDays"] ?? "30");
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim("user_id", user.Id.ToString()),
            new Claim("tenant_id", user.TenantId.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        // add roles if you want
        var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();
        foreach (var r in roles)
            claims.Add(new Claim(ClaimTypes.Role, r));

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(_key),
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshTokenString()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(
        User user,
        string ipAddress
    )
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshTokenString();

        var rt = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshDays),
        };

        _db.RefreshTokens.Add(rt);
        await _db.SaveChangesAsync();

        return (accessToken, refreshToken);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _db
            .RefreshTokens.Include(r => r.User)
            .SingleOrDefaultAsync(r => r.Token == token);
    }

    public async Task RevokeRefreshTokenAsync(
        RefreshToken token,
        string ipAddress,
        string? replacedByToken = null
    )
    {
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReplacedByToken = replacedByToken;
        _db.RefreshTokens.Update(token);
        await _db.SaveChangesAsync();
    }

    public async Task<(string AccessToken, string RefreshToken)> RefreshAsync(
        string token,
        string ipAddress
    )
    {
        var existing = await GetRefreshTokenAsync(token);
        if (existing == null || !existing.IsActive)
            throw new SecurityTokenException("Invalid refresh token");

        // rotate - create new token and revoke existing
        var user = existing.User!;
        var newRefreshToken = GenerateRefreshTokenString();

        var newTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshDays),
        };

        // revoke old
        await RevokeRefreshTokenAsync(existing, ipAddress, newRefreshToken);

        _db.RefreshTokens.Add(newTokenEntity);
        await _db.SaveChangesAsync();

        var accessToken = GenerateAccessToken(user);
        return (accessToken, newRefreshToken);
    }
}
