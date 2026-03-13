namespace MerchantDeviceManager.Domain.Entities;

/// <summary>
/// Represents a POS terminal/device belonging to a merchant.
/// </summary>
public class Device
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string? Model { get; set; }
    public DeviceStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public Merchant Merchant { get; set; } = null!;
}

/// <summary>
/// Device lifecycle status.
/// </summary>
public enum DeviceStatus
{
    Active,
    Inactive,
    Blocked
}
