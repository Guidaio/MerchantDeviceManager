using MerchantDeviceManager.Domain.Entities;
using MerchantDeviceManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MerchantDeviceManager.Web.Middleware;

/// <summary>
/// Resolves tenant and role from headers/cookies.
/// Validates merchant exists and sets HttpContext.Items for ITenantContext and IRoleContext.
/// </summary>
public class TenantResolutionMiddleware
{
    private const string TenantHeaderName = "X-Tenant-Id";
    private const string RoleHeaderName = "X-Role";
    private const string TenantCookieName = "TenantId";
    private const string RoleCookieName = "Role";
    private const string TenantItemsKey = "CurrentTenantId";
    private const string RoleItemsKey = "CurrentRole";

    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, MerchantDeviceDbContext db)
    {
        var tenantId = GetTenantIdFromRequest(context);
        if (tenantId.HasValue)
        {
            var exists = await db.Merchants.AnyAsync(m => m.Id == tenantId.Value);
            if (exists)
            {
                context.Items[TenantItemsKey] = tenantId.Value;
                context.Response.Cookies.Append(TenantCookieName, tenantId.Value.ToString(), new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                });
            }
        }

        var role = GetRoleFromRequest(context);
        if (role.HasValue)
        {
            context.Items[RoleItemsKey] = role.Value;
            context.Response.Cookies.Append(RoleCookieName, role.Value.ToString(), new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });
        }

        await _next(context);
    }

    private static Guid? GetTenantIdFromRequest(HttpContext context)
    {
        var header = context.Request.Headers[TenantHeaderName].FirstOrDefault();
        if (!string.IsNullOrEmpty(header) && Guid.TryParse(header, out var fromHeader))
            return fromHeader;

        if (context.Request.Cookies.TryGetValue(TenantCookieName, out var cookie) && Guid.TryParse(cookie, out var fromCookie))
            return fromCookie;

        return null;
    }

    private static OperatorRole? GetRoleFromRequest(HttpContext context)
    {
        var header = context.Request.Headers[RoleHeaderName].FirstOrDefault();
        if (!string.IsNullOrEmpty(header) && Enum.TryParse<OperatorRole>(header, true, out var fromHeader))
            return fromHeader;

        if (context.Request.Cookies.TryGetValue(RoleCookieName, out var cookie) && Enum.TryParse<OperatorRole>(cookie, true, out var fromCookie))
            return fromCookie;

        return null;
    }
}
