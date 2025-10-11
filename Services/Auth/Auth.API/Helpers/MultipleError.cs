using System.Net;

namespace Auth.API.Helpers;

[HttpCodeAttribute(HttpStatusCode.BadRequest)]
public class MultipleError : DomainError
{
    public override string Code => "multiple_error";

    public IEnumerable<IDomainError> Errors { get; }

    public MultipleError(IEnumerable<IDomainError> errors)
    {
        Errors = errors;
    }
}
