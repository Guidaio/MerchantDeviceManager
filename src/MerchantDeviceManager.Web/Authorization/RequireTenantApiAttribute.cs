using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MerchantDeviceManager.Web.Authorization;

/// <summary>
/// For API controllers: returns 401 with ProblemDetails when tenant is not set.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireTenantApiAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var tenantContext = context.HttpContext.RequestServices.GetService(typeof(Services.ITenantContext)) as Services.ITenantContext;

        if (tenantContext is null || !tenantContext.HasTenant)
        {
            context.Result = new ObjectResult(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Title = "Tenant required",
                Status = StatusCodes.Status401Unauthorized,
                Detail = "Provide X-Tenant-Id header with a valid merchant GUID."
            })
            { StatusCode = StatusCodes.Status401Unauthorized };
        }
    }
}
