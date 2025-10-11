using System.Net;

namespace Auth.API.Helpers;

[HttpCode(HttpStatusCode.NotFound)]
public class NotFound(string Message) : DomainError
{
    public override string Code => "not_found";

    public string Message { get; } = Message;
}
