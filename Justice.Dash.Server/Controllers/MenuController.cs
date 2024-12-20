using Justice.Dash.Server.DataModels;
using Justice.Dash.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Justice.Dash.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class MenuController : ControllerBase
{
    private readonly ILogger<MenuController> _logger;
    private readonly DashboardDbContext _context;
    private readonly IConfiguration _config;

    public MenuController(ILogger<MenuController> logger, DashboardDbContext context, IConfiguration config)
    {
        _logger = logger;
        _context = context;
        _config = config;
    }

    [HttpGet(Name = "GetMenu")]
    public async Task<IEnumerable<MenuItem>> GetAsync([FromQuery] bool full = false)
    {
        var menuItems = full switch
        {
            true => await _context.MenuItems.Include(it => it.Image).Include(it => it.VeganizedImage)
                .OrderBy(it => it.Date).ToListAsync(),
            false => await _context.MenuItems.Include(it => it.Image)
                .Include(it => it.VeganizedImage)
                .Where(it => it.Date >= DateOnly.FromDateTime(DateTime.Today))
                .OrderBy(it => it.Date)
                .ToListAsync()
        };

        var baseUrl = _config.GetValue<string>("BaseUrl");
        if (baseUrl is null) return menuItems;
        foreach (MenuItem menuItem in menuItems)
        {
            if (menuItem.Image is not null)
            {
                var url = new Uri(Path.Combine(baseUrl, menuItem.Image.Path).Replace('\\', '/'));
                menuItem.Image.Path = url.ToString();
            }
            if (menuItem.VeganizedImage is not null)
            {
                var url = new Uri(Path.Combine(baseUrl, menuItem.VeganizedImage.Path).Replace('\\', '/'));
                menuItem.VeganizedImage.Path = url.ToString();
            }
        }

        return menuItems;
    }
}