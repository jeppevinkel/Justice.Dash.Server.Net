using System.Text.Json;
using Justice.Dash.Server.DataModels;
using Justice.Dash.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Justice.Dash.Server.Services;

public class FoodAndCoService : BackgroundService
{
    private readonly ILogger<FoodAndCoService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;

    private readonly JsonSerializerOptions _jsonOptions = new()
        {PropertyNamingPolicy = JsonNamingPolicy.CamelCase};

    public FoodAndCoService(ILogger<FoodAndCoService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://www.shop.foodandco.dk");
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<DashboardDbContext>();

            List<FoodAndCoResponse> weeks = [];
            DateTime dateTime = DateTime.Now;

            for (var i = 0; i < 3; i++)
            {
                HttpResponseMessage response = await _httpClient.GetAsync(
                    $"/api/WeeklyMenu?restaurantId=1089&languageCode=da-DK&date={DateOnly.FromDateTime(dateTime).ToString("yyyy-MM-dd")}",
                    cancellationToken);
                var text = await response.Content.ReadAsStringAsync(cancellationToken);
            
                var week = JsonSerializer.Deserialize<FoodAndCoResponse>(text, _jsonOptions)!;
                weeks.Add(week);
                dateTime = dateTime.AddDays(7);
            }

            foreach (FoodAndCoResponse week in weeks)
            {
                foreach (FoodAndCoResponse.Day day in week.Days)
                {
                    DateOnly currentDate = DateOnly.FromDateTime(day.Date);
                    try
                    {
                        MenuItem? menuItem =
                            await dbContext.MenuItems.FirstOrDefaultAsync(it => it.Date == currentDate,
                                cancellationToken);

                        if (menuItem is null)
                        {
                            var foodModifiers =
                                await dbContext.FoodModifiers.ToListAsync(cancellationToken: cancellationToken);
                            menuItem = new MenuItem
                            {
                                Date = DateOnly.FromDateTime(day.Date),
                                Day = day.DayOfWeek,
                                FoodName = day.Menus.First().Menu,
                                WeekNumber = week.WeekNumber,
                                FoodModifier = foodModifiers[Random.Shared.Next(foodModifiers.Count)],
                            };

                            await dbContext.MenuItems.AddAsync(menuItem, cancellationToken);
                        }

                        if (menuItem.FoodName != day.Menus.First().Menu)
                        {
                            _logger.LogDebug("{Date} has been made dirty", day.Date);
                            menuItem.FoodName = day.Menus.First().Menu;
                            
                            menuItem.NeedsNameCorrection = true;
                            menuItem.NeedsVeganization = true;
                            menuItem.NeedsDescription = true;
                            menuItem.NeedsVeganDescription = true;
                            menuItem.NeedsFoodContents = true;
                            menuItem.NeedsImageRegeneration = true;
                            menuItem.NeedsVeganImageRegeneration = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{Message}", ex.Message);
                    }
                }
            }

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
            }

            await Task.Delay(TimeSpan.FromMinutes(1440), cancellationToken);
        }
    }
}