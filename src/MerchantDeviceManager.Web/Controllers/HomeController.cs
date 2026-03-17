using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MerchantDeviceManager.Web.Models;

namespace MerchantDeviceManager.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Forbidden(string? message = null)
    {
        ViewData["Message"] = message ?? "You do not have permission to perform this action.";
        Response.StatusCode = 403;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
