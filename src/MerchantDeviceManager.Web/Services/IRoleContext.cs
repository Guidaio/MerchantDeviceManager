using MerchantDeviceManager.Domain.Entities;

namespace MerchantDeviceManager.Web.Services;

/// <summary>
/// Provides the current operator role for the request.
/// Resolved from Role cookie by TenantResolutionMiddleware.
/// </summary>
public interface IRoleContext
{
    OperatorRole? CurrentRole { get; }
    bool IsAdmin => CurrentRole == OperatorRole.Admin;
    bool CanCreate => CurrentRole is OperatorRole.Admin or OperatorRole.Support;
    bool CanDelete => CurrentRole == OperatorRole.Admin;
}
