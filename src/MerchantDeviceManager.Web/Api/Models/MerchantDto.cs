namespace MerchantDeviceManager.Web.Api.Models;

/// <summary>
/// Merchant representation for API responses.
/// </summary>
public record MerchantDto(Guid Id, string Name, string Document, string Status);
