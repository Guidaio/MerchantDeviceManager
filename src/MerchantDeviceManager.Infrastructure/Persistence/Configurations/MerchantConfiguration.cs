using MerchantDeviceManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MerchantDeviceManager.Infrastructure.Persistence.Configurations;

internal class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.ToTable("Merchants");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Document)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(m => m.Document)
            .IsUnique();

        builder.Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.CreatedAtUtc)
            .IsRequired();

        builder.HasMany(m => m.Devices)
            .WithOne(d => d.Merchant)
            .HasForeignKey(d => d.MerchantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
