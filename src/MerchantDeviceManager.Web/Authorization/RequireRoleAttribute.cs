using MerchantDeviceManager.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MerchantDeviceManager.Web.Authorization;

/// <summary>
/// Restricts action to operators with one of the specified roles.
/// Requires tenant context; redirects to merchant selection if no tenant or insufficient role.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly OperatorRole[] _allowedRoles;

    public RequireRoleAttribute(params OperatorRole[] allowedRoles)
    {
        _allowedRoles = allowedRoles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var tenantContext = context.HttpContext.RequestServices.GetService(typeof(Services.ITenantContext)) as Services.ITenantContext;
        var roleContext = context.HttpContext.RequestServices.GetService(typeof(Services.IRoleContext)) as Services.IRoleContext;

        if (tenantContext is null || !tenantContext.HasTenant)
        {
            context.Result = new RedirectToActionResult("Index", "Merchants", null);
            return;
        }

        if (roleContext?.CurrentRole is null || !_allowedRoles.Contains(roleContext.CurrentRole.Value))
        {
            context.Result = new RedirectToActionResult("Forbidden", "Home", new { message = "Viewer role is read-only. Use Admin or Support to create devices." });
        }
    }
}
