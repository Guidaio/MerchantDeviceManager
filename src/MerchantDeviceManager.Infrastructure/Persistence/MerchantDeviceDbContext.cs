using MerchantDeviceManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MerchantDeviceManager.Infrastructure.Persistence;

public class MerchantDeviceDbContext : DbContext
{
    public MerchantDeviceDbContext(DbContextOptions<MerchantDeviceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<Device> Devices => Set<Device>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MerchantDeviceDbContext).Assembly);
    }
}
