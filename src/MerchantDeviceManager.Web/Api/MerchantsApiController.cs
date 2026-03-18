using MerchantDeviceManager.Infrastructure.Persistence;
using MerchantDeviceManager.Web.Api.Models;
using MerchantDeviceManager.Web.Models;
using MerchantDeviceManager.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MerchantDeviceManager.Web.Api;

/// <summary>
/// REST API for merchants. No tenant required for listing (used for tenant selection).
/// </summary>
[ApiController]
[Route("api/v1/merchants")]
[Produces("application/json")]
public class MerchantsApiController : ControllerBase
{
    private readonly MerchantDeviceDbContext _db;
    private readonly IMerchantCacheService _cache;

    public MerchantsApiController(MerchantDeviceDbContext db, IMerchantCacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    /// <summary>
    /// List all merchants. Used for tenant selection in integrations.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MerchantDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
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

        var dtos = merchants.Select(m => new MerchantDto(m.Id, m.Name, m.Document, m.Status));
        return Ok(dtos);
    }
}
