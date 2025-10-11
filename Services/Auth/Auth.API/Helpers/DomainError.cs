using Microsoft.AspNetCore.Identity;

namespace Auth.API.Helpers;

public interface IDomainError
{
    string Code { get; }
}

public abstract class DomainError : IDomainError
{
    public abstract string Code { get; }
}

public class ApplicationError : DomainError
{
    public ApplicationError(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public override string Code { get; }
    public string Message { get; }
}

public class CustomIdentityError : IdentityError, IDomainError
{
    public string DescriptionKey { get; set; }

    public async Task<ApplicationError> ToApplicationError() =>
        new ApplicationError(this.Code, this.Description);
}

public static class CustomIdentityErrorExtensions
{
    public static async Task<DomainError> ToApplicationErrors(
        this IEnumerable<IdentityError> errors
    )
    {
        var appErrorsTasks = errors
            .Cast<CustomIdentityError>()
            .Select(_ => _.ToApplicationError())
            .ToArray();
        var appErrors = await Task.WhenAll(appErrorsTasks);

        if (appErrors.Count() > 1)
        {
            return new MultipleError(appErrors);
        }
        return appErrors.First();
    }
}
