using System.Net;

namespace Shared.Web.Helpers;

[HttpCode(HttpStatusCode.Unauthorized)]
public class UnauthorizedError(string Message) : DomainError
{
    public override string Code => "unauthorized";

    public string Message { get; } = Message;
}
