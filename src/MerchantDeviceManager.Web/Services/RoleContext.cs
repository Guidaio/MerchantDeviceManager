using MerchantDeviceManager.Domain.Entities;

namespace MerchantDeviceManager.Web.Services;

public class RoleContext : IRoleContext
{
    private const string ItemsKey = "CurrentRole";

    public RoleContext(IHttpContextAccessor httpContextAccessor)
    {
        var context = httpContextAccessor.HttpContext;
        if (context?.Items[ItemsKey] is OperatorRole role)
            CurrentRole = role;
        else
            CurrentRole = null;
    }

    public OperatorRole? CurrentRole { get; }
}
