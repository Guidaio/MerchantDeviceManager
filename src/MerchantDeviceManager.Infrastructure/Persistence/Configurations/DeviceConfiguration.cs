using MerchantDeviceManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MerchantDeviceManager.Infrastructure.Persistence.Configurations;

internal class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Devices");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.SerialNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Model)
            .HasMaxLength(100);

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(d => d.CreatedAtUtc)
            .IsRequired();

        // SerialNumber unique per merchant (same serial can exist for different merchants)
        builder.HasIndex(d => new { d.MerchantId, d.SerialNumber })
            .IsUnique();
    }
}
