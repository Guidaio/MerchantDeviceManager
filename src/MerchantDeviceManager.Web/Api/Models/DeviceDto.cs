namespace MerchantDeviceManager.Web.Api.Models;

/// <summary>
/// Device representation for API responses.
/// </summary>
public record DeviceDto(Guid Id, Guid MerchantId, string SerialNumber, string? Model, string Status);

/// <summary>
/// Request body for creating a device.
/// </summary>
public record CreateDeviceRequest(string SerialNumber, string? Model);
