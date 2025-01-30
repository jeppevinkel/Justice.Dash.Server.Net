using Justice.Dash.Server.DataModels;
using Justice.Dash.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Justice.Dash.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class FoodModifierController : ControllerBase
{
    private readonly ILogger<FoodModifierController> _logger;
    private readonly DashboardDbContext _context;
    private readonly IConfiguration _config;
    private readonly StateService _stateService;
    
    public FoodModifierController(ILogger<FoodModifierController> logger, DashboardDbContext context, IConfiguration config,
        StateService stateService)
    {
        _logger = logger;
        _context = context;
        _config = config;
        _stateService = stateService;
    }

    [HttpGet(Name = "GetFoodModifiers")]
    public async Task<IEnumerable<FoodModifier>> GetAsync()
    {
        return await _context.FoodModifiers.ToListAsync();
    }

    [HttpGet("{id:guid}", Name = "GetFoodModifier")]
    public async Task<ActionResult<FoodModifier>> GetAsync(Guid id)
    {
        FoodModifier? foodModifier = await _context.FoodModifiers.FindAsync(id);
        
        if (foodModifier == null)
        {
            return NotFound();
        }
        
        return new ObjectResult(foodModifier);
    }

    [HttpPost(Name = "CreateFoodModifier")]
    public async Task<ActionResult<FoodModifier>> CreateAsync([FromBody] FoodModifier foodModifier)
    {
        _context.FoodModifiers.Add(foodModifier);
        await _context.SaveChangesAsync();
        
        return CreatedAtRoute("GetFoodModifier", new { id = foodModifier.Id }, foodModifier);
    }

    [HttpPut("{id:Guid}", Name = "UpdateFoodModifier")]
    public async Task<IActionResult> UpdateAsync(Guid id, FoodModifier foodModifier)
    {
        if (id != foodModifier.Id)
        {
            return BadRequest();
        }

        FoodModifier? existingModifier = await _context.FoodModifiers.FindAsync(id);
        if (existingModifier == null)
        {
            return NotFound();
        }

        existingModifier.Title = foodModifier.Title;
        existingModifier.Description = foodModifier.Description;
        
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.FoodModifiers.AnyAsync(e => e.Id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }
}