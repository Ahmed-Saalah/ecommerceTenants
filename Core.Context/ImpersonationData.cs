namespace Core.Context;

public class ImpersonationData
{
    public string? ImpersonatorId { get; }
    public string? ImpersonatorName { get; }

    public ImpersonationData(string? id, string? name)
    {
        ImpersonatorId = id;
        ImpersonatorName = name;
    }
}
