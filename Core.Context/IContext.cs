namespace Core.Context;

public interface IContext
{
    AppUser? CurrentUser { get; set; }
    ImpersonationData? ImpersonateData { get; set; }
    string? CurrentTenant { get; set; }
    string? CurrentLanguage { get; set; }
}
