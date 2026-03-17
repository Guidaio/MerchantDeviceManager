using MerchantDeviceManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MerchantDeviceManager.Web.Middleware;

/// <summary>
/// Resolves tenant from X-Tenant-Id header or TenantId cookie.
/// Validates merchant exists and sets HttpContext.Items for ITenantContext.
/// </summary>
public class TenantResolutionMiddleware
{
    private const string HeaderName = "X-Tenant-Id";
    private const string CookieName = "TenantId";
    private const string ItemsKey = "CurrentTenantId";

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
                context.Items[ItemsKey] = tenantId.Value;
                context.Response.Cookies.Append(CookieName, tenantId.Value.ToString(), new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                });
            }
        }

        await _next(context);
    }

    private static Guid? GetTenantIdFromRequest(HttpContext context)
    {
        var header = context.Request.Headers[HeaderName].FirstOrDefault();
        if (!string.IsNullOrEmpty(header) && Guid.TryParse(header, out var fromHeader))
            return fromHeader;

        if (context.Request.Cookies.TryGetValue(CookieName, out var cookie) && Guid.TryParse(cookie, out var fromCookie))
            return fromCookie;

        return null;
    }
}
