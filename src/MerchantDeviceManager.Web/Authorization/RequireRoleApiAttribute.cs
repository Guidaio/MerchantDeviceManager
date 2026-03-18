using MerchantDeviceManager.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MerchantDeviceManager.Web.Authorization;

/// <summary>
/// For API controllers: returns 401/403 with ProblemDetails when tenant or role is missing/invalid.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireRoleApiAttribute : Attribute, IAuthorizationFilter
{
    private readonly OperatorRole[] _allowedRoles;

    public RequireRoleApiAttribute(params OperatorRole[] allowedRoles)
    {
        _allowedRoles = allowedRoles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var tenantContext = context.HttpContext.RequestServices.GetService(typeof(Services.ITenantContext)) as Services.ITenantContext;
        var roleContext = context.HttpContext.RequestServices.GetService(typeof(Services.IRoleContext)) as Services.IRoleContext;

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
            return;
        }

        if (roleContext?.CurrentRole is null || !_allowedRoles.Contains(roleContext.CurrentRole.Value))
        {
            context.Result = new ObjectResult(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                Title = "Forbidden",
                Status = StatusCodes.Status403Forbidden,
                Detail = "Insufficient role. Provide X-Role header: Admin or Support."
            })
            { StatusCode = StatusCodes.Status403Forbidden };
            return;
        }
    }
}
