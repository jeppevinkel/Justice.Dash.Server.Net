using Justice.Dash.Server.DataModels;
using Justice.Dash.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Justice.Dash.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ProgressAdoController : ControllerBase
{
    private readonly ProgressAdoService _progressService;

    public ProgressAdoController(ProgressAdoService progressService)
    {
        _progressService = progressService;
    }

    [HttpGet]
    public async Task<ActionResult<ProgressAdo>> GetProgress()
    {
        ProgressAdo? progress = await _progressService.GetProgressAsync();
        if (progress == null)
        {
            return NotFound();
        }
        return Ok(progress);
    }

    [HttpPut]
    public async Task<ActionResult<ProgressAdo>> UpdateProgress([FromBody] ProgressUpdate update)
    {
        if (update.TotalItems < 0 || update.CompletedItems < 0 || update.CompletedItems > update.TotalItems)
        {
            return BadRequest("Invalid progress values");
        }

        var progress = await _progressService.UpdateProgressAsync(update.CompletedItems, update.TotalItems);
        return Ok(progress);
    }
}

public class ProgressUpdate
{
    public int CompletedItems { get; set; }
    public int TotalItems { get; set; }
}