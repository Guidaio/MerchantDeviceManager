using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MerchantDeviceManager.Infrastructure.Persistence;

namespace MerchantDeviceManager.Tests;

/// <summary>
/// Custom WebApplicationFactory that uses InMemory database for isolated integration tests.
/// Disables Redis (uses in-memory cache). Seeds demo merchants.
/// </summary>
public class MerchantDeviceManagerWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = "MerchantDeviceManager_Test_" + Guid.NewGuid().ToString("N")[..8];

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Redis", "");

        builder.ConfigureServices(services =>
        {
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<MerchantDeviceDbContext>) ||
                d.ServiceType == typeof(MerchantDeviceDbContext)).ToList();
            foreach (var d in toRemove)
                services.Remove(d);

            services.AddDbContext<MerchantDeviceDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }

    /// <summary>
    /// Creates an HttpClient with X-Tenant-Id and X-Role headers for device endpoints.
    /// </summary>
    public HttpClient CreateAuthenticatedClient(Guid tenantId, string role = "Admin")
    {
        var client = base.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
        client.DefaultRequestHeaders.Add("X-Role", role);
        return client;
    }
}
