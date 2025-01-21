using Justice.Dash.Server.DataModels;
using Justice.Dash.Server.Models;
using Justice.Dash.Server.Services;
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
    private readonly StateService _stateService;

    public MenuController(ILogger<MenuController> logger, DashboardDbContext context, IConfiguration config, StateService stateService)
    {
        _logger = logger;
        _context = context;
        _config = config;
        _stateService = stateService;
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

    [HttpGet(Name = "GetMenu"), Route("[controller]/regen")]
    public async Task<IActionResult> RegenAsync([FromQuery] string date)
    {
        var dateToMatch = DateOnly.Parse(date);
        var menuItem = await _context.MenuItems.Where(it => it.Date.CompareTo(dateToMatch) >= 0).FirstAsync();

        menuItem.Dirty = true;
        _stateService.TriggerAiTasks = true;

        var result = await _context.SaveChangesAsync();

        return Ok(result);
    }
}