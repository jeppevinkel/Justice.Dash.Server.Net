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

    public MenuController(ILogger<MenuController> logger, DashboardDbContext context, IConfiguration config,
        StateService stateService)
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
            true => await _context.MenuItems
                .OrderBy(it => it.Date).ToListAsync(),
            false => await _context.MenuItems.Where(it => it.Date >= DateOnly.FromDateTime(DateTime.Today))
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

    [HttpGet("regen/{date}", Name = "RegenMenuItem")]
    public async Task<IActionResult> RegenAsync(string date)
    {
        DateOnly dateToMatch = DateOnly.Parse(date);
        MenuItem menuItem = await _context.MenuItems.Where(it => it.Date.CompareTo(dateToMatch) >= 0).FirstAsync();

        menuItem.NeedsImageRegeneration = true;
        _stateService.TriggerAiTasks = true;

        var result = await _context.SaveChangesAsync();

        return Ok(result);
    }

    [HttpPut("{date}", Name = "UpdateMenuItem")]
    public async Task<IActionResult> UpdateAsync(string date, [FromBody] MenuItemUpdate menuItemUpdate)
    {
        DateOnly dateToMatch = DateOnly.Parse(date);
        MenuItem? menuItem = await _context.MenuItems.Where(it => it.Date.CompareTo(dateToMatch) >= 0).FirstOrDefaultAsync();
        if (menuItem == null)
            return NotFound();

        // Update manual changes
        if (menuItemUpdate.FoodName != null)
        {
            menuItem.FoodName = menuItemUpdate.FoodName;
            // When food name changes, we need to regenerate everything unless explicitly provided
            menuItem.NeedsNameCorrection = true;
            menuItem.NeedsVeganization = true;
            menuItem.NeedsDescription = true;
            menuItem.NeedsVeganDescription = true;
            menuItem.NeedsFoodContents = true;
            menuItem.NeedsImageRegeneration = true;
            menuItem.NeedsVeganImageRegeneration = true;
        }

        // Allow manual overrides without triggering regeneration
        if (menuItemUpdate.CorrectedFoodName != null)
        {
            menuItem.CorrectedFoodName = menuItemUpdate.CorrectedFoodName;
            menuItem.NeedsNameCorrection = false;
        }

        if (menuItemUpdate.VeganizedFoodName != null)
        {
            menuItem.VeganizedFoodName = menuItemUpdate.VeganizedFoodName;
            menuItem.NeedsVeganization = false;
        }

        if (menuItemUpdate.Description != null)
        {
            menuItem.Description = menuItemUpdate.Description;
            menuItem.NeedsDescription = false;
        }

        if (menuItemUpdate.VeganizedDescription != null)
        {
            menuItem.VeganizedDescription = menuItemUpdate.VeganizedDescription;
            menuItem.NeedsVeganDescription = false;
        }

        if (menuItemUpdate.FoodModifierId != null)
        {
            if (menuItemUpdate.FoodModifierId == Guid.Empty)
            {
                menuItem.FoodModifier = null;
            }
            else
            {
                FoodModifier? foodModifier = await _context.FoodModifiers.FindAsync(menuItemUpdate.FoodModifierId);
                menuItem.FoodModifier = foodModifier;
            }
            
            menuItem.NeedsImageRegeneration = true;
            menuItem.NeedsVeganImageRegeneration = true;
        }

        // Handle explicit regeneration requests
        if (menuItemUpdate.RegenerateNames)
        {
            menuItem.NeedsNameCorrection = true;
            menuItem.NeedsVeganization = true;
        }

        if (menuItemUpdate.RegenerateDescriptions)
        {
            menuItem.NeedsDescription = true;
            menuItem.NeedsVeganDescription = true;
        }

        if (menuItemUpdate.RegenerateImages)
        {
            menuItem.NeedsImageRegeneration = true;
            menuItem.NeedsVeganImageRegeneration = true;
        }

        if (menuItemUpdate.RegenerateFoodContents)
        {
            menuItem.NeedsFoodContents = true;
        }

        await _context.SaveChangesAsync();
        
        var baseUrl = _config.GetValue<string>("BaseUrl");
        if (baseUrl is null) return Ok(menuItem);
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
        
        return Ok(menuItem);
    }
}