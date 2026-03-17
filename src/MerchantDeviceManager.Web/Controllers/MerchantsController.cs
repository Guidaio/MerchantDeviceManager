using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MerchantDeviceManager.Infrastructure.Persistence;
using MerchantDeviceManager.Web.Models;

namespace MerchantDeviceManager.Web.Controllers;

public class MerchantsController : Controller
{
    private readonly MerchantDeviceDbContext _db;

    public MerchantsController(MerchantDeviceDbContext db) => _db = db;

    /// <summary>
    /// List merchants for tenant selection (no tenant filter).
    /// </summary>
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var merchants = await _db.Merchants
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .Select(m => new MerchantListModel(m.Id, m.Name, m.Document, m.Status.ToString()))
            .ToListAsync(ct);

        return View(merchants);
    }

    /// <summary>
    /// Select tenant and redirect to devices.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Select(Guid merchantId)
    {
        Response.Cookies.Append("TenantId", merchantId.ToString(), new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });
        return RedirectToAction(nameof(DevicesController.Index), "Devices");
    }
}
