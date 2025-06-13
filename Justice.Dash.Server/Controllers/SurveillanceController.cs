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
    public async Task<Dictionary<string, IEnumerable<SurveillanceResponse>>> GetAsync([FromQuery] bool full = false)
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

        // Get all day overrides that apply to the time period
        var dayOverrides = full switch
        {
            true => await _context.SurveillanceDayOverrides.ToListAsync(),
            false => await _context.SurveillanceDayOverrides
                .Where(it => it.Date.Year == dateTime.Year && 
                             it.Date >= DateOnly.FromDateTime(dateTime))
                .ToListAsync()
        };

        // Group day overrides by surveillance type
        var mdmSurveillance = surveillance.Where(it => it.Type == SurveillanceType.MDM).ToList();
        var ediSurveillance = surveillance.Where(it => it.Type == SurveillanceType.EDI).ToList();
        
        var mdmDayOverrides = dayOverrides.Where(it => it.Type == SurveillanceType.MDM).ToList();
        var ediDayOverrides = dayOverrides.Where(it => it.Type == SurveillanceType.EDI).ToList();

        // Map to response objects
        var mdmResponses = MapSurveillanceResponses(mdmSurveillance, mdmDayOverrides);
        var ediResponses = MapSurveillanceResponses(ediSurveillance, ediDayOverrides);

        var categorizedSurveillance =
            new Dictionary<string, IEnumerable<SurveillanceResponse>>
            {
                {"MDM", mdmResponses},
                {"EDI", ediResponses}
            };
        
        return categorizedSurveillance;
    }

    private List<SurveillanceResponse> MapSurveillanceResponses(
        List<Surveillance> surveillanceList, 
        List<SurveillanceDayOverride> dayOverrides)
    {
        var responses = new List<SurveillanceResponse>();
        
        foreach (var surveillance in surveillanceList)
        {
            var response = new SurveillanceResponse
            {
                WeeklySurveillance = surveillance,
                DayOverrides = dayOverrides
                    .Where(d => IsDayInSurveillanceWeek(d.Date, surveillance.Week, surveillance.Year))
                    .ToList()
            };
            
            responses.Add(response);
        }
        
        return responses;
    }
    
    private bool IsDayInSurveillanceWeek(DateOnly date, int week, int year)
    {
        // Create a DateTime from the DateOnly
        var dateTime = date.ToDateTime(TimeOnly.MinValue);
        
        // Get the calendar and week number
        Calendar calendar = new CultureInfo("da-DK").Calendar;
        var dayWeek = calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        
        return date.Year == year && dayWeek == week;
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
    
    [HttpGet("day-overrides", Name = "GetDayOverrides")]
    public async Task<ActionResult<IEnumerable<SurveillanceDayOverride>>> GetDayOverridesAsync([FromQuery] DateOnly? fromDate = null)
    {
        IQueryable<SurveillanceDayOverride> query = _context.SurveillanceDayOverrides;
        
        if (fromDate.HasValue)
        {
            query = query.Where(o => o.Date >= fromDate.Value);
        }
        
        var overrides = await query.OrderBy(o => o.Date).ToListAsync();
        return Ok(overrides);
    }

    [HttpPost("day-overrides", Name = "CreateDayOverride")]
    public async Task<ActionResult<SurveillanceDayOverride>> CreateDayOverrideAsync(SurveillanceDayOverride dayOverride)
    {
        try
        {
            // Check if an override already exists for this day and type
            var existingOverride = await _context.SurveillanceDayOverrides
                .FirstOrDefaultAsync(o => o.Date == dayOverride.Date && o.Type == dayOverride.Type);
            
            if (existingOverride != null)
            {
                return Conflict($"An override for {dayOverride.Type} on {dayOverride.Date} already exists.");
            }
            
            // Link to weekly surveillance if it exists
            Calendar calendar = new CultureInfo("da-DK").Calendar;
            var weekNumber = calendar.GetWeekOfYear(
                dayOverride.Date.ToDateTime(TimeOnly.MinValue), 
                CalendarWeekRule.FirstDay, 
                DayOfWeek.Monday);
            
            var weeklySurveillance = await _context.Surveillance
                .FirstOrDefaultAsync(s => s.Year == dayOverride.Date.Year && 
                                        s.Week == weekNumber && 
                                        s.Type == dayOverride.Type);
            
            if (weeklySurveillance != null)
            {
                dayOverride.WeeklySurveillanceId = weeklySurveillance.Id;
            }
            
            _context.SurveillanceDayOverrides.Add(dayOverride);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetDayOverridesAsync), null, dayOverride);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating surveillance day override");
            return StatusCode(500, "An error occurred while creating the surveillance day override.");
        }
    }
    
    [HttpPut("day-overrides/{id:guid}", Name = "UpdateDayOverride")]
    public async Task<ActionResult<SurveillanceDayOverride>> UpdateDayOverrideAsync(Guid id, SurveillanceDayOverride dayOverride)
    {
        if (id != dayOverride.Id)
        {
            return BadRequest("The ID in the URL does not match the ID in the request body.");
        }

        var existingOverride = await _context.SurveillanceDayOverrides.FindAsync(id);
        
        if (existingOverride == null)
        {
            return NotFound($"Surveillance day override with ID {id} not found.");
        }

        // Check if updating would create a duplicate
        if (existingOverride.Date != dayOverride.Date || existingOverride.Type != dayOverride.Type)
        {
            var duplicateExists = await _context.SurveillanceDayOverrides
                .AnyAsync(o => o.Id != id && o.Date == dayOverride.Date && o.Type == dayOverride.Type);
            
            if (duplicateExists)
            {
                return Conflict($"An override for {dayOverride.Type} on {dayOverride.Date} already exists.");
            }
        }

        // Update properties
        existingOverride.Type = dayOverride.Type;
        existingOverride.Date = dayOverride.Date;
        existingOverride.Responsible = dayOverride.Responsible;
        
        // Update the weekly surveillance reference if the date or type changed
        if (existingOverride.Date != dayOverride.Date || existingOverride.Type != dayOverride.Type)
        {
            Calendar calendar = new CultureInfo("da-DK").Calendar;
            var weekNumber = calendar.GetWeekOfYear(
                dayOverride.Date.ToDateTime(TimeOnly.MinValue), 
                CalendarWeekRule.FirstDay, 
                DayOfWeek.Monday);
            
            var weeklySurveillance = await _context.Surveillance
                .FirstOrDefaultAsync(s => s.Year == dayOverride.Date.Year && 
                                        s.Week == weekNumber && 
                                        s.Type == dayOverride.Type);
            
            existingOverride.WeeklySurveillanceId = weeklySurveillance?.Id;
        }

        try
        {
            await _context.SaveChangesAsync();
            return Ok(existingOverride);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating surveillance day override");
            return StatusCode(500, "An error occurred while updating the surveillance day override.");
        }
    }

    [HttpDelete("day-overrides/{id:guid}", Name = "DeleteDayOverride")]
    public async Task<ActionResult> DeleteDayOverrideAsync(Guid id)
    {
        var override = await _context.SurveillanceDayOverrides.FindAsync(id);
        
        if (override == null)
        {
            return NotFound($"Surveillance day override with ID {id} not found.");
        }

        _context.SurveillanceDayOverrides.Remove(override);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}