using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Context;

public static class ContextMiddleware
{
    public static IApplicationBuilder UseAppContext(this IApplicationBuilder app)
    {
        return app.Use(
            async (context, next) =>
            {
                var ctx = context.RequestServices.GetRequiredService<IContext>();

                var headers = context.Request.Headers;

                headers.TryGetValue(ContextHttpHeaders.UserId, out var userIdStr);
                headers.TryGetValue(ContextHttpHeaders.UserName, out var username);
                headers.TryGetValue(ContextHttpHeaders.UserRole, out var role);
                headers.TryGetValue(ContextHttpHeaders.SessionId, out var sessionId);
                headers.TryGetValue(ContextHttpHeaders.Tenant, out var tenant);
                headers.TryGetValue(ContextHttpHeaders.Language, out var lang);
                headers.TryGetValue(ContextHttpHeaders.ImpersonatedById, out var impersonatedId);
                headers.TryGetValue(
                    ContextHttpHeaders.ImpersonatedByName,
                    out var impersonatedName
                );

                int.TryParse(userIdStr, out var userId);

                if (userId != 0)
                {
                    ctx.CurrentUser = new AppUser(userId, username, role, sessionId);
                }

                if (!string.IsNullOrWhiteSpace(tenant))
                    ctx.CurrentTenant = tenant;

                if (!string.IsNullOrWhiteSpace(lang))
                    ctx.CurrentLanguage = lang;

                if (
                    !string.IsNullOrWhiteSpace(impersonatedId)
                    || !string.IsNullOrWhiteSpace(impersonatedName)
                )
                {
                    ctx.ImpersonateData = new ImpersonationData(impersonatedId, impersonatedName);
                }

                await next();
            }
        );
    }
}
