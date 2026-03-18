using MerchantDeviceManager.Web.Models;

namespace MerchantDeviceManager.Web.Services;

/// <summary>
/// Caches merchant data in Redis (or in-memory when Redis not configured).
/// Reduces DB load for frequently accessed merchant list and names.
/// </summary>
public interface IMerchantCacheService
{
    Task<IReadOnlyList<MerchantListModel>> GetMerchantListAsync(Func<Task<IReadOnlyList<MerchantListModel>>> loadFromDb, CancellationToken ct = default);
    Task<string?> GetMerchantNameAsync(Guid merchantId, Func<Task<string?>> loadFromDb, CancellationToken ct = default);
    Task InvalidateMerchantListAsync(CancellationToken ct = default);
}
