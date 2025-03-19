using Justice.Dash.Server.DataModels;
using Justice.Dash.Server.Models;
using Microsoft.EntityFrameworkCore;
using Netatmo;
using Netatmo.Models;
using Netatmo.Models.Client;
using Netatmo.Models.Client.Weather;
using Netatmo.Models.Client.Weather.StationsData;
using Netatmo.Models.Client.Weather.StationsData.DashboardData;
using Newtonsoft.Json;
using NodaTime;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Justice.Dash.Server.Services;

/// <summary>
/// Service for interacting with the Netatmo API to retrieve weather data
/// </summary>
public class NetatmoService : IHostedService
{
    private readonly ILogger<NetatmoService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;
    private readonly IServiceProvider _serviceProvider;
    private Client? _client;
    private Timer? _timer;
    private NetatmoConfig _config = new();
    
    /// <summary>
    /// Constructor for NetatmoService
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="env">WebHost environment</param>
    /// <param name="context">Database context</param>
    public NetatmoService(ILogger<NetatmoService> logger, IConfiguration configuration, IWebHostEnvironment env, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _env = env;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Start the service and initialize the Netatmo client
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Netatmo Service");
        
        try
        {
            // Load configuration
            _config = _configuration.GetSection("Netatmo").Get<NetatmoConfig>() ?? new NetatmoConfig();

            string accessToken;
            string refreshToken;

            if (File.Exists(CredentialsFilePath))
            {
                var tokenData = await File.ReadAllTextAsync(CredentialsFilePath, cancellationToken);
                var token = JsonSerializer.Deserialize<NetatmoToken>(tokenData);

                accessToken = token!.AccessToken;
                refreshToken = token.RefreshToken;
            }
            else
            {
                accessToken = _config.AccessToken;
                refreshToken = _config.RefreshToken;
            }
            
            if (string.IsNullOrEmpty(_config.ClientId) || string.IsNullOrEmpty(_config.ClientSecret))
            {
                _logger.LogWarning("Netatmo API credentials not configured. Service will not fetch data");
                return;
            }
            
            // Initialize client
            _client = new Client(SystemClock.Instance, "https://api.netatmo.com/",
                _config.ClientId,
                _config.ClientSecret);
            
            _client.ProvideOAuth2Token(accessToken, refreshToken);
            await RefreshToken(cancellationToken);
            
            // Update once immediately then schedule regular updates
            _ = FetchWeatherDataAsync();
            
            // Update weather data every 5 minutes
            _timer = new Timer(async _ => await FetchWeatherDataAsync(), null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Netatmo service");
        }
    }

    /// <summary>
    /// Stop the service
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Netatmo Service");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }
    
    private async Task FetchWeatherDataAsync()
    {
        if (_client is null)
        {
            _logger.LogWarning("Netatmo client not initialized");
            return;
        }

        if (_client.CredentialManager.CredentialToken.ExpiresAt > SystemClock.Instance.GetCurrentInstant())
        {
            await RefreshToken();
        }
        
        try
        {
            _logger.LogInformation("Fetching weather data from Netatmo API");
            
            // Get station data
            DataResponse<GetStationsDataBody> stationsData = await _client.Weather.GetStationsData();
            
            // Find the main station or the configured station if provided
            Device? station = null;
            
            if (!string.IsNullOrEmpty(_config.StationId))
            {
                station = stationsData.Body.Devices.FirstOrDefault(d => d.Id == _config.StationId);
            }
            
            // Use the first station if no specific one is configured or found
            station ??= stationsData.Body.Devices.FirstOrDefault();
            
            if (station == null)
            {
                _logger.LogWarning("No weather station found in the Netatmo account");
                return;
            }
            
            // Find rain module if available
            var rainModule = station.Modules.FirstOrDefault(m => m.Type.Equals("NAModule3", StringComparison.OrdinalIgnoreCase));
            var rainGaugeDashBoardData = rainModule?.GetDashboardData<RainGaugeDashBoardData>();
            
            // Find outdoor module for other metrics if available
            var outdoorModule = station.Modules.FirstOrDefault(m => m.Type.Equals("NAModule1", StringComparison.OrdinalIgnoreCase));
            var outdoorDashboardData = outdoorModule?.GetDashboardData<OutdoorDashBoardData>();

            AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            await using var context = scope.ServiceProvider.GetRequiredService<DashboardDbContext>();

            Weather? weatherData = await context.Weather.FirstOrDefaultAsync();

            if (weatherData is null)
            {
                weatherData = new Weather();
                await context.Weather.AddAsync(weatherData);
            }

            // Update weather data
            weatherData.LastUpdate = DateTime.UtcNow;
            
            // Check for rain data
            if (rainModule is not null && rainGaugeDashBoardData is not null)
            {
                weatherData.RainAmount = rainGaugeDashBoardData.Rain;
                
                // Consider it's raining if rain amount is greater than 0.1 mm/hour
                weatherData.IsRaining = rainGaugeDashBoardData.Rain > 0.1;
            }
            
            // Add outdoor temperature and humidity if available
            if (outdoorModule is not null && outdoorDashboardData is not null)
            {
                weatherData.Temperature = outdoorDashboardData.Temperature;
                weatherData.Humidity = outdoorDashboardData.HumidityPercent;
            }
            
            _logger.LogInformation("Weather data updated. Is raining: {IsRaining}, Rain amount: {RainAmount} mm/h", 
                weatherData.IsRaining, 
                weatherData.RainAmount);

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather data from Netatmo");
        }
    }

    private async Task RefreshToken(CancellationToken cancellationToken = default)
    {
        if (_client is null)
        {
            throw new Exception("Client must be defined before refreshing the token.");
        }
        
        await _client.RefreshToken();
        
        Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "credentials"));

        var tokenData = JsonSerializer.Serialize(new NetatmoToken(_client.CredentialManager.CredentialToken));
        await File.WriteAllTextAsync(CredentialsFilePath, tokenData, cancellationToken);
    }

    private string CredentialsFilePath => Path.Combine(_env.ContentRootPath, "credentials", "netatmo.json");
}