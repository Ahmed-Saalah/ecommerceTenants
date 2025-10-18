using System.Net;

namespace Customers.API.Helpers;

[HttpCode(HttpStatusCode.NotFound)]
public sealed class NotFoundError(string message) : DomainError
{
    public override string Code => "not_found";

    public string Message => message;
}
