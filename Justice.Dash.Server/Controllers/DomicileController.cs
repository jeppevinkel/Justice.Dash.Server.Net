using Justice.Dash.Server.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Justice.Dash.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class DomicileController : ControllerBase
{
    private readonly ILogger<DomicileController> _logger;
    private readonly DashboardDbContext _context;
    private readonly IConfiguration _config;

    public DomicileController(ILogger<DomicileController> logger, DashboardDbContext context, IConfiguration config)
    {
        _logger = logger;
        _context = context;
        _config = config;
    }

    [HttpGet(Name = "GetDomicile")]
    public async Task<List<Photo>>  Get()
    {
        var photos = await _context.Photos.OrderByDescending(it => it.ImageUpdateDate).ToListAsync();

        return photos;
    }
}