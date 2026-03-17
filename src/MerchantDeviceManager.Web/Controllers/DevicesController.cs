using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MerchantDeviceManager.Domain.Entities;
using MerchantDeviceManager.Infrastructure.Persistence;
using MerchantDeviceManager.Web.Models;
using MerchantDeviceManager.Web.Services;

namespace MerchantDeviceManager.Web.Controllers;

public class DevicesController : Controller
{
    private readonly MerchantDeviceDbContext _db;
    private readonly ITenantContext _tenant;

    public DevicesController(MerchantDeviceDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    /// <summary>
    /// List devices for current tenant. Redirects to merchant selection if no tenant.
    /// </summary>
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (!_tenant.HasTenant)
            return RedirectToAction(nameof(MerchantsController.Index), "Merchants");

        var merchantId = _tenant.CurrentMerchantId!.Value;
        var devices = await _db.Devices
            .AsNoTracking()
            .Where(d => d.MerchantId == merchantId)
            .OrderBy(d => d.SerialNumber)
            .Select(d => new DeviceListModel(d.Id, d.SerialNumber, d.Model ?? "-", d.Status.ToString()))
            .ToListAsync(ct);

        var merchant = await _db.Merchants.AsNoTracking().FirstAsync(m => m.Id == merchantId, ct);
        ViewData["MerchantName"] = merchant.Name;

        return View(devices);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        if (!_tenant.HasTenant)
            return RedirectToAction(nameof(MerchantsController.Index), "Merchants");

        var merchant = await _db.Merchants.AsNoTracking()
            .FirstAsync(m => m.Id == _tenant.CurrentMerchantId!.Value, ct);
        ViewData["MerchantName"] = merchant.Name;
        return View(new CreateDeviceModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDeviceModel createModel, CancellationToken ct)
    {
        if (!_tenant.HasTenant)
            return RedirectToAction(nameof(MerchantsController.Index), "Merchants");

        var merchantId = _tenant.CurrentMerchantId!.Value;

        var exists = await _db.Devices.AnyAsync(d => d.MerchantId == merchantId && d.SerialNumber == createModel.SerialNumber, ct);
        if (exists)
        {
            ModelState.AddModelError(nameof(createModel.SerialNumber), "Serial number already exists for this merchant.");
            return View(createModel);
        }

        var device = new Device
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            SerialNumber = createModel.SerialNumber.Trim(),
            Model = string.IsNullOrWhiteSpace(createModel.Model) ? null : createModel.Model.Trim(),
            Status = DeviceStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Devices.Add(device);
        await _db.SaveChangesAsync(ct);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SwitchTenant()
    {
        Response.Cookies.Delete("TenantId");
        return RedirectToAction(nameof(MerchantsController.Index), "Merchants");
    }
}
