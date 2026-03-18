using System.Net;
using System.Net.Http.Json;
using MerchantDeviceManager.Web.Api.Models;
using Xunit;

namespace MerchantDeviceManager.Tests.Integration;

public class MerchantsApiTests : IClassFixture<MerchantDeviceManagerWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MerchantsApiTests(MerchantDeviceManagerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task List_Returns200WithMerchants()
    {
        var response = await _client.GetAsync("/api/v1/merchants");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var merchants = await response.Content.ReadFromJsonAsync<List<MerchantDto>>();
        Assert.NotNull(merchants);
        Assert.True(merchants.Count >= 2, "Seeder adds at least 2 merchants");
        Assert.Contains(merchants, m => m.Name == "Acme Store");
        Assert.Contains(merchants, m => m.Name == "Beta POS");
    }
}
