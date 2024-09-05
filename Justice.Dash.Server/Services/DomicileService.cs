namespace Justice.Dash.Server.Services;

public class DomicileService : BackgroundService
{
    private readonly ILogger<DomicileService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IWebHostEnvironment _env;

    public DomicileService(ILogger<DomicileService> logger, IServiceProvider serviceProvider,
        IConfiguration configuration,
        IWebHostEnvironment env)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _env = env;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await DoWork(cancellationToken);
    }

    private async Task DoWork(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            
            
            await Task.Delay(TimeSpan.FromHours(2), cancellationToken);
        }
    }
}