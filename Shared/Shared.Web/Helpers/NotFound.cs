using System.Net;

namespace Shared.Web.Helpers;

[HttpCode(HttpStatusCode.NotFound)]
public class NotFound(string Message) : DomainError
{
    public override string Code => "not_found";

    public string Message { get; } = Message;
}
