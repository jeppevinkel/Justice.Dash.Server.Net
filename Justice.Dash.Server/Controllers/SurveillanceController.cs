using System.Globalization;
using Justice.Dash.Server.DataModels;
using Justice.Dash.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Justice.Dash.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class SurveillanceController : ControllerBase
{
    private readonly ILogger<SurveillanceController> _logger;
    private readonly DashboardDbContext _context;
    private readonly IConfiguration _config;

    public SurveillanceController(ILogger<SurveillanceController> logger, DashboardDbContext context,
        IConfiguration config)
    {
        _logger = logger;
        _context = context;
        _config = config;
    }

    [HttpGet(Name = "GetSurveillance")]
    public async Task<Dictionary<string, IEnumerable<Surveillance>>> GetAsync([FromQuery] bool full = false)
    {
        DateTime dateTime = DateTime.Today;
        Calendar calendar = new CultureInfo("da-DK").Calendar;
        var currentWeek = calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

        var surveillance = full switch
        {
            true => await _context.Surveillance.OrderBy(it => it.Year).ThenBy(it => it.Week).ToListAsync(),
            false => await _context.Surveillance.Where(it => it.Year == dateTime.Year && it.Week >= currentWeek)
                .OrderBy(it => it.Year).ThenBy(it => it.Week)
                .ToListAsync()
        };

        var categorizedSurveillance =
            new Dictionary<string, IEnumerable<Surveillance>>
            {
                {"MDM", surveillance.Where(it => it.Type == SurveillanceType.MDM)},
                {"EDI", surveillance.Where(it => it.Type == SurveillanceType.EDI)}
            };
        return categorizedSurveillance;
    }
}