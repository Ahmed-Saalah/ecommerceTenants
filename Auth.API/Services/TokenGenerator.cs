using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auth.API.DbContexts;
using Auth.API.Models;
using Microsoft.IdentityModel.Tokens;

namespace Auth.API.Services;

public interface ITokenGenerator
{
    (string AccessToken, string RefreshToken, DateTime ExpiresAt) Generate(User user);
}

public class TokenGenerator : ITokenGenerator
{
    private readonly IConfiguration _config;
    private readonly AuthDbContext _dbContext;

    public TokenGenerator(IConfiguration config, AuthDbContext dbContext)
    {
        _config = config;
        _dbContext = dbContext;
    }

    public (string AccessToken, string RefreshToken, DateTime ExpiresAt) Generate(User user)
    {
        var jwtKey = _config["Jwt:Key"];
        var jwtIssuer = _config["Jwt:Issuer"];
        var jwtAudience = _config["Jwt:Audience"];
        var expires = DateTime.UtcNow.AddMinutes(30);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtKey!);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim("displayName", user.DisplayName ?? ""),
            new Claim("role", "User"),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(token);

        var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        _dbContext.RefreshTokens.Add(
            new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            }
        );

        _dbContext.SaveChanges();

        return (accessToken, refreshToken, expires);
    }
}
