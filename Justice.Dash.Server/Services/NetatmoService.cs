using Justice.Dash.Server.Models;
using Netatmo;
using Netatmo.Models.Client;
using Netatmo.Models.Client.Weather;
using Netatmo.Models.Client.Weather.StationsData;
using Netatmo.Models.Client.Weather.StationsData.DashboardData;
using NodaTime;

namespace Justice.Dash.Server.Services;

/// <summary>
/// Service for interacting with the Netatmo API to retrieve weather data
/// </summary>
public class NetatmoService : IHostedService
{
    private readonly ILogger<NetatmoService> _logger;
    private readonly IConfiguration _configuration;
    private Client? _client;
    private Timer? _timer;
    private WeatherResponse _lastWeatherData = new() 
    { 
        IsRaining = false, 
        LastUpdate = DateTime.UtcNow 
    };
    private NetatmoConfig _config = new();
    
    /// <summary>
    /// Constructor for NetatmoService
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="configuration">Configuration instance</param>
    public NetatmoService(ILogger<NetatmoService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Returns the most recent weather data
    /// </summary>
    /// <returns>Weather response containing rain information</returns>
    public WeatherResponse GetWeather()
    {
        return _lastWeatherData;
    }

    /// <summary>
    /// Start the service and initialize the Netatmo client
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Netatmo Service");
        
        try
        {
            // Load configuration
            _config = _configuration.GetSection("Netatmo").Get<NetatmoConfig>() ?? new NetatmoConfig();
            
            if (string.IsNullOrEmpty(_config.ClientId) || string.IsNullOrEmpty(_config.ClientSecret))
            {
                _logger.LogWarning("Netatmo API credentials not configured. Service will not fetch data.");
                return Task.CompletedTask;
            }
            
            // Initialize client
            _client = new Client(SystemClock.Instance, "https://api.netatmo.com/",
                _config.StationId,
                _config.ClientSecret);
            
            _client.ProvideOAuth2Token(_config.AccessToken, _config.RefreshToken);
            
            // Update once immediately then schedule regular updates
            _ = FetchWeatherDataAsync();
            
            // Update weather data every 5 minutes
            _timer = new Timer(async _ => await FetchWeatherDataAsync(), null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Netatmo service");
        }
        
        return Task.CompletedTask;
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
        if (_client == null)
        {
            _logger.LogWarning("Netatmo client not initialized");
            return;
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
            
            // Update weather data
            _lastWeatherData = new WeatherResponse
            {
                LastUpdate = DateTime.UtcNow
            };
            
            // Check for rain data
            if (rainModule is not null && rainGaugeDashBoardData is not null)
            {
                _lastWeatherData.RainAmount = rainGaugeDashBoardData.Rain;
                
                // Consider it's raining if rain amount is greater than 0.1 mm/hour
                _lastWeatherData.IsRaining = rainGaugeDashBoardData.Rain > 0.1;
            }
            
            // Add outdoor temperature and humidity if available
            if (outdoorModule is not null && outdoorDashboardData is not null)
            {
                _lastWeatherData.Temperature = outdoorDashboardData.Temperature;
                _lastWeatherData.Humidity = outdoorDashboardData.HumidityPercent;
            }
            
            _logger.LogInformation("Weather data updated. Is raining: {IsRaining}, Rain amount: {RainAmount} mm/h", 
                _lastWeatherData.IsRaining, 
                _lastWeatherData.RainAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather data from Netatmo");
        }
    }
}