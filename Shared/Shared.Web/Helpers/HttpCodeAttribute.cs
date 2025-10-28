using System.Net;

namespace Shared.Web.Helpers;

[AttributeUsage(AttributeTargets.Class)]
public class HttpCodeAttribute : Attribute
{
    public HttpStatusCode Code { get; protected set; }

    public HttpCodeAttribute(HttpStatusCode code)
    {
        Code = code;
    }
}
