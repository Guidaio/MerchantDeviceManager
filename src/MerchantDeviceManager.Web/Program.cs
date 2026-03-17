using MerchantDeviceManager.Infrastructure.Persistence;
using MerchantDeviceManager.Web.Middleware;
using MerchantDeviceManager.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddDbContext<MerchantDeviceDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=merchantdevices.db"));

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseMiddleware<TenantResolutionMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MerchantDeviceDbContext>();
    await db.Database.EnsureCreatedAsync();
    await MerchantDeviceSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
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
