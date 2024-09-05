using Justice.Dash.Server.DataModels;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using OpenAI.Images;

namespace Justice.Dash.Server.Services;

public class AiService : BackgroundService
{
    private readonly ILogger<AiService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IWebHostEnvironment _env;
    private readonly ChatClient _chatClient;
    private readonly ImageClient _imageClient;
    private readonly string[] _foodTypes = ["fisk", "svinekød", "kød", "laktosefri", "fjerkræ", "vegansk"];

    public AiService(ILogger<AiService> logger, IServiceProvider serviceProvider, IConfiguration configuration,
        IWebHostEnvironment env)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _env = env;
        _chatClient = new ChatClient(model: "gpt-4o", configuration.GetValue<string>("Tokens:OpenAI") ?? string.Empty);
        _imageClient = new ImageClient(model: "dall-e-3",
            configuration.GetValue<string>("Tokens:OpenAI") ?? string.Empty);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await DoWork(cancellationToken);
    }

    private async Task DoWork(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<DashboardDbContext>();

            var menuItems = await dbContext.MenuItems.Include(it => it.Image).Where(it => it.Dirty)
                .ToListAsync(cancellationToken);

            foreach (MenuItem menuItem in menuItems)
            {
                await Task.WhenAll(CorrectFoodName(menuItem), DescribeFood(menuItem),
                    ListFoodContents(menuItem, _foodTypes), GenerateImage(menuItem, dbContext, cancellationToken));
                // await CorrectFoodName(menuItem);
                // await DescribeFood(menuItem);
                // await ListFoodContents(menuItem, _foodTypes);
                // await GenerateImage(menuItem, dbContext, cancellationToken);

                menuItem.Dirty = false;
                _logger.LogDebug("Fixed the name and description of {Name} for {Date}", menuItem.FoodName,
                    menuItem.Date);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await Task.Delay(TimeSpan.FromMinutes(60), cancellationToken);
        }
    }

    private async Task CorrectFoodName(MenuItem menuItem)
    {
        ChatCompletion completion = await _chatClient.CompleteChatAsync(
            new SystemChatMessage(
                "Din opgave er at omskrive navnet på madretter til at være grammatisk korrekt og stavet rigtigt. Undgå at slutte med tegnsætning. Forkortelser må gerne bruges eller bibeholdes. Du skal kun svare med navnet og intet andet."),
            new UserChatMessage($"Retten hedder \"{menuItem.FoodName}\""));

        menuItem.CorrectedFoodName = completion.ToString();
    }

    private async Task DescribeFood(MenuItem menuItem)
    {
        ChatCompletion completion = await _chatClient.CompleteChatAsync(
            new SystemChatMessage(
                "Din opgave er at beskrive madretter på en kort måde. Du skal svare med kun beskrivelsen og intet andet."),
            new UserChatMessage($"Retten hedder \"{menuItem.FoodName}\""));

        menuItem.Description = completion.ToString();
    }

    private async Task ListFoodContents(MenuItem menuItem, IEnumerable<string> foodTypes)
    {
        List<string> contents = [];
        foreach (var foodType in foodTypes)
        {
            ChatCompletion completion = await _chatClient.CompleteChatAsync(
                new SystemChatMessage(
                    $"Din opgave er at afgøre om der er {foodType} i denne ret. Hvis den indeholder {foodType} skal du svare med \"ja\" og ikke andet. Hvis ikke den indeholder {foodType} skal du svare med \"nej\" og ikke andet."),
                new UserChatMessage($"Retten hedder \"{menuItem.FoodName}\""));

            if (completion.ToString().Equals("ja", StringComparison.CurrentCultureIgnoreCase))
            {
                contents.Add(foodType);
            }
        }

        menuItem.FoodContents = contents;
    }

    private async Task GenerateImage(MenuItem menuItem, DashboardDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var basePath = Path.Combine(_env.ContentRootPath, "wwwroot");
        if (menuItem.Image is not null)
        {
            var path = Path.Combine(basePath, menuItem.Image.Path);
            if (File.Exists(path))
            {
                File.Delete(path);
                dbContext.Remove(menuItem.Image);
                menuItem.Image = null;
            }
        }

        var prompt = $"Food called \"{menuItem.FoodName}\"";
        GeneratedImage image = await _imageClient.GenerateImageAsync(prompt,
            new ImageGenerationOptions
            {
                Quality = GeneratedImageQuality.High,
                Size = GeneratedImageSize.W1792xH1024,
                ResponseFormat = GeneratedImageFormat.Bytes
            }, cancellationToken);

        menuItem.Image = new Image
        {
            Path = "",
            Prompt = prompt,
            RevisedPrompt = image.RevisedPrompt
        };

        var imagePath = Path.Combine("images", "food", $"{menuItem.Image.Id}.png");
        await using FileStream stream = File.OpenWrite(Path.Combine(basePath, imagePath));
        await image.ImageBytes.ToStream().CopyToAsync(stream, cancellationToken);

        menuItem.Image.Path = imagePath.Replace('\\', '/');
    }
}