using System.ComponentModel.DataAnnotations;

namespace MerchantDeviceManager.Web.Models;

public class CreateDeviceModel
{
    [Required]
    [StringLength(50)]
    [Display(Name = "Serial Number")]
    public string SerialNumber { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Model")]
    public string? Model { get; set; }
}
