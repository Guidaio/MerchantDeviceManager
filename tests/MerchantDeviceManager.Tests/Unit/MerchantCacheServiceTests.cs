using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using MerchantDeviceManager.Web.Models;
using MerchantDeviceManager.Web.Services;
using Xunit;

namespace MerchantDeviceManager.Tests.Unit;

public class MerchantCacheServiceTests
{
    private static MerchantCacheService CreateSut()
    {
        var services = new ServiceCollection();
        services.AddDistributedMemoryCache();
        var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDistributedCache>();
        return new MerchantCacheService(cache);
    }

    [Fact]
    public async Task GetMerchantListAsync_CacheMiss_CallsLoadFromDbAndStoresInCache()
    {
        var sut = CreateSut();
        var expected = new List<MerchantListModel>
        {
            new(Guid.NewGuid(), "Acme", "12.345.678/0001-90", "Active")
        };

        var result = await sut.GetMerchantListAsync(() => Task.FromResult<IReadOnlyList<MerchantListModel>>(expected));

        Assert.Equal(expected, result);
        var secondCall = await sut.GetMerchantListAsync(() => throw new InvalidOperationException("Should not be called"));
        Assert.Equal(expected[0].Name, secondCall[0].Name);
    }

    [Fact]
    public async Task GetMerchantListAsync_CacheHit_DoesNotCallLoadFromDb()
    {
        var sut = CreateSut();
        var expected = new List<MerchantListModel>
        {
            new(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Acme", "12.345.678/0001-90", "Active")
        };
        await sut.GetMerchantListAsync(() => Task.FromResult<IReadOnlyList<MerchantListModel>>(expected));

        var loadCalled = false;
        var result = await sut.GetMerchantListAsync(() =>
        {
            loadCalled = true;
            return Task.FromResult<IReadOnlyList<MerchantListModel>>(new List<MerchantListModel>());
        });

        Assert.False(loadCalled);
        Assert.Equal("Acme", result[0].Name);
    }

    [Fact]
    public async Task GetMerchantNameAsync_CacheMiss_CallsLoadFromDbAndStoresInCache()
    {
        var sut = CreateSut();
        var merchantId = Guid.NewGuid();
        var expectedName = "Beta POS";

        var result = await sut.GetMerchantNameAsync(merchantId, () => Task.FromResult<string?>(expectedName));

        Assert.Equal(expectedName, result);
        var cached = await sut.GetMerchantNameAsync(merchantId, () => throw new InvalidOperationException("Should not be called"));
        Assert.Equal(expectedName, cached);
    }

    [Fact]
    public async Task GetMerchantNameAsync_CacheHit_ReturnsCachedValue()
    {
        var sut = CreateSut();
        var merchantId = Guid.NewGuid();
        await sut.GetMerchantNameAsync(merchantId, () => Task.FromResult<string?>("Cached Name"));

        var loadCalled = false;
        var result = await sut.GetMerchantNameAsync(merchantId, () =>
        {
            loadCalled = true;
            return Task.FromResult<string?>("From DB");
        });

        Assert.False(loadCalled);
        Assert.Equal("Cached Name", result);
    }

    [Fact]
    public async Task InvalidateMerchantListAsync_RemovesCacheEntry()
    {
        var sut = CreateSut();
        var list = new List<MerchantListModel> { new(Guid.NewGuid(), "X", "doc", "Active") };
        await sut.GetMerchantListAsync(() => Task.FromResult<IReadOnlyList<MerchantListModel>>(list));
        await sut.InvalidateMerchantListAsync();

        var loadCalled = false;
        await sut.GetMerchantListAsync(() =>
        {
            loadCalled = true;
            return Task.FromResult<IReadOnlyList<MerchantListModel>>(new List<MerchantListModel>());
        });

        Assert.True(loadCalled);
    }
}
