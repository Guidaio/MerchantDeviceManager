using MerchantDeviceManager.Infrastructure.Persistence;
using MerchantDeviceManager.Web.Middleware;
using MerchantDeviceManager.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "MerchantDevice:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IRoleContext, RoleContext>();
builder.Services.AddScoped<IMerchantCacheService, MerchantCacheService>();
builder.Services.AddDbContext<MerchantDeviceDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=merchantdevices.db"));

builder.Services.AddControllersWithViews();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MerchantDeviceManager API",
        Version = "v1",
        Description = "REST API for merchant and device management. Back-office fintech/POS. Requires X-Tenant-Id for device endpoints; X-Role (Admin/Support) for create."
    });
    options.OperationFilter<MerchantDeviceManager.Web.Api.SwaggerTenantHeaderOperationFilter>();
});

var app = builder.Build();

app.UseMiddleware<TenantResolutionMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MerchantDeviceDbContext>();
    await db.Database.EnsureCreatedAsync();
    await MerchantDeviceSeeder.SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
