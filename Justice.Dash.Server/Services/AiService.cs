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

            foreach (MenuItem item in await dbContext.MenuItems.Include(it => it.VeganizedImage)
                               .Where(it => it.VeganizedImage == null || it.VeganizedFoodName == null).Where(it => it.Date > DateOnly.FromDateTime(new DateTime())).ToListAsync(cancellationToken))
            {
                await CorrectVeganFoodName(item);
                await Task.WhenAll(DescribeVeganFood(item),
                    GenerateVeganImage(item, dbContext, cancellationToken));
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            var menuItems = await dbContext.MenuItems.Include(it => it.Image).Include(it => it.VeganizedImage).Where(it => it.Dirty)
                .ToListAsync(cancellationToken);

            foreach (MenuItem menuItem in menuItems)
            {
                await CorrectFoodName(menuItem);
                await Task.WhenAll(DescribeFood(menuItem),
                    ListFoodContents(menuItem, _foodTypes), GenerateImages(menuItem, dbContext, cancellationToken));

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

        await CorrectVeganFoodName(menuItem);
    }

    private async Task CorrectVeganFoodName(MenuItem menuItem)
    {
        ChatCompletion completion = await _chatClient.CompleteChatAsync(
            new SystemChatMessage(
                "Din opgave er at omskrive navnet på madretter til at være grammatisk korrekt og stavet rigtigt. Undgå at slutte med tegnsætning. Forkortelser må gerne bruges eller bibeholdes. Du skal kun svare med navnet og intet andet. Retten skal være omskrevet til at være vegansk, der må ikke være referencer til kød i retten."),
            new UserChatMessage($"Retten hedder \"{menuItem.FoodDisplayName}\""));
        
        menuItem.VeganizedFoodName = completion.ToString();
    }

    private async Task DescribeFood(MenuItem menuItem)
    {
        ChatCompletion completion = await _chatClient.CompleteChatAsync(
            new SystemChatMessage(
                "Din opgave er at beskrive madretter på en kort måde. Du skal svare med kun beskrivelsen og intet andet."),
            new UserChatMessage($"Retten hedder \"{menuItem.FoodDisplayName}\""));

        menuItem.Description = completion.ToString();

        await DescribeVeganFood(menuItem);
    }

    private async Task DescribeVeganFood(MenuItem menuItem)
    {
        ChatCompletion completion = await _chatClient.CompleteChatAsync(
            new SystemChatMessage(
                "Din opgave er at beskrive madretter på en kort måde. Du skal svare med kun beskrivelsen og intet andet. Det er en vegansk ret."),
            new UserChatMessage($"Retten hedder \"{menuItem.VeganizedFoodName ?? menuItem.FoodDisplayName}\""));

        menuItem.VeganizedDescription = completion.ToString();
    }

    private async Task ListFoodContents(MenuItem menuItem, IEnumerable<string> foodTypes)
    {
        List<string> contents = [];
        foreach (var foodType in foodTypes)
        {
            ChatCompletion completion = await _chatClient.CompleteChatAsync(
                new SystemChatMessage(
                    $"Din opgave er at afgøre om der er {foodType} i denne ret. Hvis den indeholder {foodType} skal du svare med \"ja\" og ikke andet. Hvis ikke den indeholder {foodType} skal du svare med \"nej\" og ikke andet."),
                new UserChatMessage($"Retten hedder \"{menuItem.FoodDisplayName}\""));

            if (completion.ToString().Equals("ja", StringComparison.CurrentCultureIgnoreCase))
            {
                contents.Add(foodType);
            }
        }

        menuItem.FoodContents = contents;
    }

    private async Task GenerateImages(MenuItem menuItem, DashboardDbContext dbContext,
        CancellationToken cancellationToken = default)
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

        await GenerateVeganImage(menuItem, dbContext, cancellationToken);

        var prompt = $"Food called \"{menuItem.FoodDisplayName}\"";
        menuItem.Image = await GenerateImage(prompt, Path.Combine("images", "food"), cancellationToken);
    }

    private async Task GenerateVeganImage(MenuItem menuItem, DashboardDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        var basePath = Path.Combine(_env.ContentRootPath, "wwwroot");
        if (menuItem.VeganizedImage is not null)
        {
            var path = Path.Combine(basePath, menuItem.VeganizedImage.Path);
            if (File.Exists(path))
            {
                File.Delete(path);
                dbContext.Remove(menuItem.VeganizedImage);
                menuItem.VeganizedImage = null;
            }
        }
        
        var veganizedPrompt = $"Food called \"{menuItem.VeganizedFoodName ?? menuItem.FoodDisplayName}\", the food is vegan.";
        menuItem.VeganizedImage = await GenerateImage(veganizedPrompt, Path.Combine("images", "food", "vegan"), cancellationToken);
    }

    private async Task<Image> GenerateImage(string prompt, string folderPath, CancellationToken cancellationToken = default)
    {
        var image = new Image()
        {
            Path = "",
            Prompt = prompt,
        };
        var basePath = Path.Combine(_env.ContentRootPath, "wwwroot");
        var imagePath = Path.Combine(folderPath, $"{image.Id}.png");
        var fullPath = Path.Combine(basePath, imagePath);
        Directory.CreateDirectory(Path.Combine(basePath, folderPath));
        
        GeneratedImage generatedImage = await _imageClient.GenerateImageAsync(prompt,
            new ImageGenerationOptions
            {
                Quality = GeneratedImageQuality.High,
                Size = GeneratedImageSize.W1792xH1024,
                ResponseFormat = GeneratedImageFormat.Bytes
            }, cancellationToken);
        
        await using FileStream stream = File.OpenWrite(fullPath);
        await generatedImage.ImageBytes.ToStream().CopyToAsync(stream, cancellationToken);

        image.Path = imagePath;
        image.RevisedPrompt = generatedImage.RevisedPrompt;

        return image;
    }
}