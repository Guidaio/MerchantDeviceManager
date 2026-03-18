using MerchantDeviceManager.Domain.Entities;
using MerchantDeviceManager.Infrastructure.Persistence;
using MerchantDeviceManager.Web.Api.Models;
using MerchantDeviceManager.Web.Authorization;
using MerchantDeviceManager.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MerchantDeviceManager.Web.Api;

/// <summary>
/// REST API for devices. Requires X-Tenant-Id header. Create requires Admin or Support role (X-Role).
/// </summary>
[ApiController]
[Route("api/v1/devices")]
[Produces("application/json")]
public class DevicesApiController : ControllerBase
{
    private readonly MerchantDeviceDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IMerchantCacheService _cache;

    public DevicesApiController(MerchantDeviceDbContext db, ITenantContext tenant, IMerchantCacheService cache)
    {
        _db = db;
        _tenant = tenant;
        _cache = cache;
    }

    /// <summary>
    /// List devices for the current tenant. Requires X-Tenant-Id header.
    /// </summary>
    [HttpGet]
    [RequireTenantApi]
    [ProducesResponseType(typeof(IEnumerable<DeviceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var merchantId = _tenant.CurrentMerchantId!.Value;
        var devices = await _db.Devices
            .AsNoTracking()
            .Where(d => d.MerchantId == merchantId)
            .OrderBy(d => d.SerialNumber)
            .Select(d => new DeviceDto(d.Id, d.MerchantId, d.SerialNumber, d.Model, d.Status.ToString()))
            .ToListAsync(ct);

        return Ok(devices);
    }

    /// <summary>
    /// Create a device for the current tenant. Requires X-Tenant-Id and X-Role (Admin or Support).
    /// </summary>
    [HttpPost]
    [RequireRoleApi(OperatorRole.Admin, OperatorRole.Support)]
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateDeviceRequest request, CancellationToken ct)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.SerialNumber))
        {
            return BadRequest(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = StatusCodes.Status400BadRequest,
                Detail = "SerialNumber is required."
            });
        }

        var merchantId = _tenant.CurrentMerchantId!.Value;
        var serialNumber = request.SerialNumber.Trim();
        var model = string.IsNullOrWhiteSpace(request.Model) ? null : request.Model.Trim();

        var exists = await _db.Devices.AnyAsync(d => d.MerchantId == merchantId && d.SerialNumber == serialNumber, ct);
        if (exists)
        {
            return BadRequest(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Serial number already exists for this merchant."
            });
        }

        var device = new Device
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            SerialNumber = serialNumber,
            Model = model,
            Status = DeviceStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Devices.Add(device);
        await _db.SaveChangesAsync(ct);
        await _cache.InvalidateMerchantListAsync(ct);

        var dto = new DeviceDto(device.Id, device.MerchantId, device.SerialNumber, device.Model, device.Status.ToString());
        return CreatedAtAction(nameof(GetById), new { id = device.Id }, dto);
    }

    /// <summary>
    /// Get a device by ID. Requires X-Tenant-Id. Returns 404 if device belongs to another tenant.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequireTenantApi]
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var merchantId = _tenant.CurrentMerchantId!.Value;
        var device = await _db.Devices
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id && d.MerchantId == merchantId, ct);

        if (device is null)
            return NotFound(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = "Device not found or does not belong to current tenant."
            });

        return Ok(new DeviceDto(device.Id, device.MerchantId, device.SerialNumber, device.Model, device.Status.ToString()));
    }
}
