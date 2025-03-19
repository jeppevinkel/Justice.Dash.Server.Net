using Justice.Dash.Server.DataModels;
using Justice.Dash.Server.Models;
using Justice.Dash.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Justice.Dash.Server.Controllers;

/// <summary>
/// Controller for retrieving weather data from Netatmo
/// </summary>
[ApiController]
[Route("[controller]")]
public class WeatherController : ControllerBase
{
    private readonly NetatmoService _netatmoService;
    private readonly ILogger<WeatherController> _logger;
    private readonly DashboardDbContext _context;

    /// <summary>
    /// Constructor for WeatherController
    /// </summary>
    /// <param name="netatmoService">Service for accessing Netatmo weather data</param>
    /// <param name="logger">Logger instance</param>
    public WeatherController(NetatmoService netatmoService, ILogger<WeatherController> logger, DashboardDbContext context)
    {
        _netatmoService = netatmoService;
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Get current weather data from Netatmo weather station
    /// </summary>
    /// <returns>Weather data including if it's raining</returns>
    [HttpGet]
    public async Task<ActionResult<Weather>> GetWeather()
    {
        try
        {
            return Ok(await _context.Weather.FirstOrDefaultAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weather data");
            return StatusCode(500, "Error retrieving weather data");
        }
    }

    /// <summary>
    /// Check if it's currently raining
    /// </summary>
    /// <returns>True if it's raining, false otherwise</returns>
    [HttpGet("isRaining")]
    public async Task<ActionResult<bool>> IsRaining()
    {
        try
        {
            var weather = await _context.Weather.FirstOrDefaultAsync();
            return Ok(weather.IsRaining);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if it's raining");
            return StatusCode(500, "Error checking if it's raining");
        }
    }
}