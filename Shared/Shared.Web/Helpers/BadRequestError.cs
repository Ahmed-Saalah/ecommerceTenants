using System.Net;

namespace Shared.Web.Helpers;

[HttpCode(HttpStatusCode.BadRequest)]
public class BadRequestError(string Message) : DomainError
{
    public override string Code => "bad_request";

    public string Message { get; } = Message;
}
