using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using MerchantDeviceManager.Web.Models;

namespace MerchantDeviceManager.Web.Services;

public class MerchantCacheService : IMerchantCacheService
{
    private const string MerchantListKey = "merchants:list";
    private const string MerchantNameKeyPrefix = "merchant:name:";
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(5);
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IDistributedCache _cache;

    public MerchantCacheService(IDistributedCache cache) => _cache = cache;

    public async Task<IReadOnlyList<MerchantListModel>> GetMerchantListAsync(Func<Task<IReadOnlyList<MerchantListModel>>> loadFromDb, CancellationToken ct = default)
    {
        var cached = await _cache.GetStringAsync(MerchantListKey, ct);
        if (cached is not null)
        {
            var list = JsonSerializer.Deserialize<List<MerchantListModel>>(cached, JsonOptions);
            if (list is not null)
                return list;
        }

        var fromDb = await loadFromDb();
        await _cache.SetStringAsync(MerchantListKey, JsonSerializer.Serialize(fromDb, JsonOptions),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = DefaultTtl }, ct);
        return fromDb;
    }

    public async Task<string?> GetMerchantNameAsync(Guid merchantId, Func<Task<string?>> loadFromDb, CancellationToken ct = default)
    {
        var key = MerchantNameKeyPrefix + merchantId.ToString("N");
        var cached = await _cache.GetStringAsync(key, ct);
        if (cached is not null)
            return cached;

        var fromDb = await loadFromDb();
        if (fromDb is not null)
            await _cache.SetStringAsync(key, fromDb, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = DefaultTtl }, ct);
        return fromDb;
    }

    public Task InvalidateMerchantListAsync(CancellationToken ct = default) =>
        _cache.RemoveAsync(MerchantListKey, ct);
}
