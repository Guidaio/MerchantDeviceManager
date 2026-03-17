namespace MerchantDeviceManager.Web.Services;

public class TenantContext : ITenantContext
{
    private const string ItemsKey = "CurrentTenantId";

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        var context = httpContextAccessor.HttpContext;
        if (context?.Items[ItemsKey] is Guid merchantId)
            CurrentMerchantId = merchantId;
        else
            CurrentMerchantId = null;
    }

    public Guid? CurrentMerchantId { get; }
}
