namespace MerchantDeviceManager.Web.Services;

/// <summary>
/// Provides the current tenant (Merchant) for the request.
/// Resolved from X-Tenant-Id header or TenantId cookie by TenantResolutionMiddleware.
/// </summary>
public interface ITenantContext
{
    Guid? CurrentMerchantId { get; }
    bool HasTenant => CurrentMerchantId.HasValue;
}
