namespace MerchantDeviceManager.Domain.Entities;

/// <summary>
/// Represents a merchant (tenant) in the system. Each merchant has one or more POS devices.
/// </summary>
public class Merchant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty; // CNPJ, CPF, or tax ID
    public MerchantStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<Device> Devices { get; set; } = new List<Device>();
}

/// <summary>
/// Merchant lifecycle status.
/// </summary>
public enum MerchantStatus
{
    Active,
    Inactive,
    Suspended
}
