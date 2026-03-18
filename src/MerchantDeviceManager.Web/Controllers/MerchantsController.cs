using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MerchantDeviceManager.Infrastructure.Persistence;
using MerchantDeviceManager.Web.Models;
using MerchantDeviceManager.Web.Services;

namespace MerchantDeviceManager.Web.Controllers;

public class MerchantsController : Controller
{
    private readonly MerchantDeviceDbContext _db;
    private readonly IMerchantCacheService _cache;

    public MerchantsController(MerchantDeviceDbContext db, IMerchantCacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    /// <summary>
    /// List merchants for tenant selection. Cached in Redis (5 min TTL).
    /// </summary>
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var merchants = await _cache.GetMerchantListAsync(async () =>
        {
            var list = await _db.Merchants
                .AsNoTracking()
                .OrderBy(m => m.Name)
                .Select(m => new MerchantListModel(m.Id, m.Name, m.Document, m.Status.ToString()))
                .ToListAsync(ct);
            return list;
        }, ct);

        return View(merchants);
    }

    /// <summary>
    /// Select tenant and role, then redirect to devices.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Select(Guid merchantId, Domain.Entities.OperatorRole role)
    {
        Response.Cookies.Append("TenantId", merchantId.ToString(), new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });
        Response.Cookies.Append("Role", role.ToString(), new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });
        return RedirectToAction(nameof(DevicesController.Index), "Devices");
    }
}
