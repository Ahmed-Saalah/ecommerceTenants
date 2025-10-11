namespace Core.Contexts;

public interface IUserContext
{
    int UserId { get; }
    int TenantId { get; }
}
