using System.Net;

namespace Auth.API.Helpers;

[HttpCode(HttpStatusCode.BadRequest)]
public class BadRequestError(string Message) : DomainError
{
    public override string Code => "bad_request";

    public string Message { get; } = Message;
}
