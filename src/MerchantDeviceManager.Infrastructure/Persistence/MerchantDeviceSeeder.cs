using MerchantDeviceManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MerchantDeviceManager.Infrastructure.Persistence;

/// <summary>
/// Seeds demo merchants for multi-tenant demo.
/// </summary>
public static class MerchantDeviceSeeder
{
    public static async Task SeedAsync(MerchantDeviceDbContext db)
    {
        if (await db.Merchants.AnyAsync())
            return;

        var merchants = new[]
        {
            new Merchant
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Acme Store",
                Document = "12.345.678/0001-90",
                Status = MerchantStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            },
            new Merchant
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Beta POS",
                Document = "98.765.432/0001-10",
                Status = MerchantStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            }
        };

        db.Merchants.AddRange(merchants);
        await db.SaveChangesAsync();
    }
}
