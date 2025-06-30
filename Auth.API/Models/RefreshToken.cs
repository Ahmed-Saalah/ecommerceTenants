namespace Auth.API.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; }

    // Foreign key
    public int UserId { get; set; }
    public User User { get; set; } = default!;
}
