namespace Auth.API.Models.Constants;

public sealed class GuestData
{
    public static readonly string Username = $"guest_{Guid.NewGuid()}";
    public static readonly string Email = $"guest_{Guid.NewGuid()}@guest.com";
    public static readonly string Password = Guid.NewGuid().ToString(); // use a secure policy if needed
    public static readonly string PhoneNumber = $"+1000{DateTime.UtcNow.Ticks % 10000000000}";
    public static readonly string DisplayName = Guid.NewGuid().ToString();
}
