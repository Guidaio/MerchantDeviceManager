using System.Net;
using System.Net.Http.Json;
using MerchantDeviceManager.Web.Api.Models;
using Xunit;

namespace MerchantDeviceManager.Tests.Integration;

public class DevicesApiTests : IClassFixture<MerchantDeviceManagerWebApplicationFactory>
{
    private static readonly Guid AcmeMerchantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly MerchantDeviceManagerWebApplicationFactory _factory;

    public DevicesApiTests(MerchantDeviceManagerWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task List_WithoutTenant_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/devices");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_WithTenant_Returns200()
    {
        var client = _factory.CreateAuthenticatedClient(AcmeMerchantId);
        var response = await client.GetAsync("/api/v1/devices");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var devices = await response.Content.ReadFromJsonAsync<List<DeviceDto>>();
        Assert.NotNull(devices);
    }

    [Fact]
    public async Task Create_WithValidRequest_Returns201()
    {
        var client = _factory.CreateAuthenticatedClient(AcmeMerchantId, "Admin");
        var request = new CreateDeviceRequest("SN-" + Guid.NewGuid().ToString("N")[..8], "Model X");

        var response = await client.PostAsJsonAsync("/api/v1/devices", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var device = await response.Content.ReadFromJsonAsync<DeviceDto>();
        Assert.NotNull(device);
        Assert.Equal(request.SerialNumber, device.SerialNumber);
        Assert.Equal(AcmeMerchantId, device.MerchantId);
        Assert.Equal("Active", device.Status);
    }

    [Fact]
    public async Task Create_WithoutRole_Returns403()
    {
        var client = _factory.CreateAuthenticatedClient(AcmeMerchantId, "Viewer");
        var request = new CreateDeviceRequest("SN-ViewerTest", null);

        var response = await client.PostAsJsonAsync("/api/v1/devices", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_DuplicateSerialNumber_Returns400()
    {
        var client = _factory.CreateAuthenticatedClient(AcmeMerchantId, "Admin");
        var serialNumber = "SN-Dup-" + Guid.NewGuid().ToString("N")[..6];
        var request = new CreateDeviceRequest(serialNumber, null);

        var first = await client.PostAsJsonAsync("/api/v1/devices", request);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/v1/devices", request);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingDevice_Returns200()
    {
        var client = _factory.CreateAuthenticatedClient(AcmeMerchantId, "Admin");
        var createRequest = new CreateDeviceRequest("SN-Get-" + Guid.NewGuid().ToString("N")[..6], null);
        var createResponse = await client.PostAsJsonAsync("/api/v1/devices", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<DeviceDto>();
        Assert.NotNull(created);

        var getResponse = await client.GetAsync($"/api/v1/devices/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var device = await getResponse.Content.ReadFromJsonAsync<DeviceDto>();
        Assert.NotNull(device);
        Assert.Equal(created.Id, device.Id);
    }

    [Fact]
    public async Task GetById_NonExistent_Returns404()
    {
        var client = _factory.CreateAuthenticatedClient(AcmeMerchantId);
        var response = await client.GetAsync($"/api/v1/devices/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
