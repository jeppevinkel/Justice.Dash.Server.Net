using System.Globalization;
using Justice.Dash.Server.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    [HttpPost(Name = "CreateSurveillance")]
    public async Task<ActionResult<Surveillance>> CreateAsync(Surveillance surveillance)
    {
        try
        {
            _context.Surveillance.Add(surveillance);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAsync), new { id = surveillance.Id }, surveillance);
        }
        catch (DbUpdateException ex)
        {
            // Handle the case when the surveillance entry already exists
            if (ex.InnerException?.Message.Contains("unique constraint") == true)
            {
                return Conflict("A surveillance entry with the same Type, Week, and Year already exists.");
            }
            
            _logger.LogError(ex, "Error creating surveillance entry");
            return StatusCode(500, "An error occurred while creating the surveillance entry.");
        }
    }

    [HttpPut("{id:guid}", Name = "UpdateSurveillance")]
    public async Task<ActionResult<Surveillance>> UpdateAsync(Guid id, Surveillance surveillance)
    {
        if (id != surveillance.Id)
        {
            return BadRequest("The ID in the URL does not match the ID in the request body.");
        }

        var existingSurveillance = await _context.Surveillance.FindAsync(id);
        
        if (existingSurveillance == null)
        {
            return NotFound($"Surveillance entry with ID {id} not found.");
        }

        // Update properties
        existingSurveillance.Type = surveillance.Type;
        existingSurveillance.Week = surveillance.Week;
        existingSurveillance.Year = surveillance.Year;
        existingSurveillance.Responsible = surveillance.Responsible;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(existingSurveillance);
        }
        catch (DbUpdateException ex)
        {
            // Handle unique constraint violation
            if (ex.InnerException?.Message.Contains("unique constraint") == true)
            {
                return Conflict("A surveillance entry with the same Type, Week, and Year already exists.");
            }
            
            _logger.LogError(ex, "Error updating surveillance entry");
            return StatusCode(500, "An error occurred while updating the surveillance entry.");
        }
    }

    [HttpDelete("{id:guid}", Name = "DeleteSurveillance")]
    public async Task<ActionResult> DeleteAsync(Guid id)
    {
        var surveillance = await _context.Surveillance.FindAsync(id);
        
        if (surveillance == null)
        {
            return NotFound($"Surveillance entry with ID {id} not found.");
        }

        _context.Surveillance.Remove(surveillance);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}