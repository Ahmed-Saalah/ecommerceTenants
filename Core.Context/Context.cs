namespace Core.Context;

public class Context : IContext
{
    public AppUser? CurrentUser { get; set; }
    public ImpersonationData? ImpersonateData { get; set; }
    public string? CurrentTenant { get; set; }
    public string? CurrentLanguage { get; set; }
}
