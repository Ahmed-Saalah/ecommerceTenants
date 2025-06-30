namespace Core.Context;

public class AppUser
{
    public int Id { get; }
    public string? Username { get; }
    public string? Role { get; }
    public string? SessionId { get; }

    public AppUser(int id, string? username, string? role, string? sessionId)
    {
        Id = id;
        Username = username;
        Role = role;
        SessionId = sessionId;
    }
}
