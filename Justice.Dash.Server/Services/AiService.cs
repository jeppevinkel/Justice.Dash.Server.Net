using Justice.Dash.Server.DataModels;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using OpenAI.Images;
using System.IO;
using System.Text;

namespace Justice.Dash.Server.Services;

public class AiService : BackgroundService
{
    private readonly ILogger<AiService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IWebHostEnvironment _env;
    private readonly StateService _stateService;
    private readonly ChatClient _chatClient;
    private readonly ImageClient _imageClient;
    private readonly string[] _foodTypes = ["fisk", "svinekød", "kød", "laktosefri", "fjerkræ", "vegansk"];

    public AiService(ILogger<AiService> logger, IServiceProvider serviceProvider, IConfiguration configuration,
        IWebHostEnvironment env, StateService stateService)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _env = env;
        _chatClient = new ChatClient(model: "gpt-4o", configuration.GetValue<string>("Tokens:OpenAI") ?? string.Empty);
        _imageClient = new ImageClient(model: "dall-e-3",
            configuration.GetValue<string>("Tokens:OpenAI") ?? string.Empty);
        _stateService = stateService;
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

            var menuItems = await dbContext.MenuItems
                .Include(it => it.Image)
                .Include(it => it.VeganizedImage)
                .Where(it => 
                    it.NeedsNameCorrection || 
                    it.NeedsVeganization || 
                    it.NeedsDescription || 
                    it.NeedsVeganDescription || 
                    it.NeedsFoodContents || 
                    it.NeedsImageRegeneration || 
                    it.NeedsVeganImageRegeneration || 
                    it.NeedsRecipeGeneration)
                .ToListAsync(cancellationToken);
            
            foreach (MenuItem menuItem in menuItems)
            {
                try
                {
                    await ProcessMenuItem(menuItem, dbContext, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing menu item {Id}", menuItem.Id);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
        }
    }
    
    private async Task ProcessMenuItem(MenuItem menuItem, DashboardDbContext dbContext, CancellationToken cancellationToken)
    {
        if (menuItem.NeedsNameCorrection)
        {
            await CorrectFoodName(menuItem);
            menuItem.NeedsNameCorrection = false;
        }

        if (menuItem.NeedsVeganization)
        {
            await CorrectVeganFoodName(menuItem);
            menuItem.NeedsVeganization = false;
        }

        if (menuItem.NeedsDescription)
        {
            await DescribeFood(menuItem);
            menuItem.NeedsDescription = false;
        }

        if (menuItem.NeedsVeganDescription)
        {
            await DescribeVeganFood(menuItem);
            menuItem.NeedsVeganDescription = false;
        }

        if (menuItem.NeedsFoodContents)
        {
            await ListFoodContents(menuItem, _foodTypes);
            menuItem.NeedsFoodContents = false;
        }

        if (menuItem.NeedsImageRegeneration)
        {
            await GenerateRegularImage(menuItem, dbContext, cancellationToken);
            menuItem.NeedsImageRegeneration = false;
        }

        if (menuItem.NeedsVeganImageRegeneration)
        {
            await GenerateVeganImage(menuItem, dbContext, cancellationToken);
            menuItem.NeedsVeganImageRegeneration = false;
        }
        
        if (menuItem.NeedsRecipeGeneration)
        {
            await GenerateRecipe(menuItem);
            menuItem.NeedsRecipeGeneration = false;
        }

        _logger.LogDebug("Processed updates for menu item {Name} for {Date}", menuItem.FoodName, menuItem.Date);
    }

    private async Task CorrectFoodName(MenuItem menuItem)
    {
        ChatCompletion completion = await _chatClient.CompleteChatAsync(
            new SystemChatMessage(
                "Din opgave er at omskrive navnet på madretter til at være grammatisk korrekt og stavet rigtigt. Undgå at slutte med tegnsætning. Forkortelser må gerne bruges eller bibeholdes. Du skal kun svare med navnet og intet andet."),
            new UserChatMessage($"Retten hedder \"{menuItem.FoodName}\""));

        menuItem.CorrectedFoodName = completion.Content[0].Text;

        await CorrectVeganFoodName(menuItem);
    }

    private async Task CorrectVeganFoodName(MenuItem menuItem)
    {
        ChatCompletion completion = await _chatClient.CompleteChatAsync(
            new SystemChatMessage(
                "Din opgave er at omskrive navnet på madretter til at være grammatisk korrekt og stavet rigtigt. Undgå at slutte med tegnsætning. Forkortelser må gerne bruges eller bibeholdes. Du skal kun svare med navnet og intet andet. Retten skal være omskrevet til at være vegansk, der må ikke være referencer til kød i retten."),
            new UserChatMessage($"Retten hedder \"{menuItem.FoodDisplayName}\""));

        menuItem.VeganizedFoodName = completion.Content[0].Text;
    }

    private async Task DescribeFood(MenuItem menuItem)
    {
        ChatCompletion completion = await _chatClient.CompleteChatAsync(
            new SystemChatMessage(
                "Din opgave er at beskrive madretter på en kort måde. Du skal svare med kun beskrivelsen og intet andet."),
            new UserChatMessage($"Retten hedder \"{menuItem.FoodDisplayName}\""));

        menuItem.Description = completion.Content[0].Text;

        await DescribeVeganFood(menuItem);
    }

    private async Task DescribeVeganFood(MenuItem menuItem)
    {
        ChatCompletion completion = await _chatClient.CompleteChatAsync(
            new SystemChatMessage(
                "Din opgave er at beskrive madretter på en kort måde. Du skal svare med kun beskrivelsen og intet andet. Det er en vegansk ret."),
            new UserChatMessage($"Retten hedder \"{menuItem.VeganizedFoodName ?? menuItem.FoodDisplayName}\""));

        menuItem.VeganizedDescription = completion.Content[0].Text;
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

            if (completion.Content[0].Text.Equals("ja", StringComparison.CurrentCultureIgnoreCase))
            {
                contents.Add(foodType);
            }
        }

        menuItem.FoodContents = contents;
    }

    private async Task GenerateRegularImage(MenuItem menuItem, DashboardDbContext dbContext,
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

        var prompt = $"Photorealistic, Food called \"{menuItem.FoodDisplayName}\"";
        if (menuItem.Description is not null)
        {
            prompt += $" and described as \"{menuItem.Description}\"";
        }

        if (menuItem.FoodModifier is not null)
        {
            prompt += $" {menuItem.FoodModifier.Description}";
        }

        menuItem.Image = await GenerateImage(prompt, Path.Combine("images", "food"), cancellationToken);
        
        // After generating the image, set the flag to generate a recipe based on the image
        menuItem.NeedsRecipeGeneration = true;
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

        var veganizedPrompt =
            $"Photorealistic, Food called \"{menuItem.VeganizedFoodName ?? menuItem.FoodDisplayName}\", the food is vegan.";
        if (menuItem.VeganizedDescription is not null)
        {
            veganizedPrompt += $" and described as \"{menuItem.VeganizedDescription}\"";
        }

        if (menuItem.FoodModifier is not null)
        {
            veganizedPrompt += $" {menuItem.FoodModifier.Description}";
        }

        menuItem.VeganizedImage =
            await GenerateImage(veganizedPrompt, Path.Combine("images", "food", "vegan"), cancellationToken);
    }

    private void RemoveImage(Image image, DashboardDbContext dbContext)
    {
        var basePath = Path.Combine(_env.ContentRootPath, "wwwroot");
        var path = Path.Combine(basePath, image.Path);
        if (!File.Exists(path)) return;
        File.Delete(path);
        dbContext.Remove(image);
    }

    private async Task GenerateRecipe(MenuItem menuItem)
    {
        // Check if we need to wait for the image to be generated first
        if (menuItem.Image == null && menuItem.NeedsImageRegeneration)
        {
            // Skip for now, we'll generate the recipe after the image is generated
            return;
        }
        
        string foodName = menuItem.FoodDisplayName;
        
        // Get the path to the image file
        var basePath = Path.Combine(_env.ContentRootPath, "wwwroot");
        var imagePath = Path.Combine(basePath, menuItem.Image?.Path ?? "");
        
        if (menuItem.Image?.Path == null || !File.Exists(imagePath))
        {
            _logger.LogWarning("Image file not found at {Path} for recipe generation", imagePath);
            
            // Fallback to using the revised prompt if image is not available
            string imagePrompt = menuItem.Image?.RevisedPrompt ?? "";
            
            ChatCompletion completion = await _chatClient.CompleteChatAsync(
                new SystemChatMessage(
                    "Your task is to create a detailed recipe for a dish. The recipe should match the content of the image that was generated, even if it seems impractical to actually make. Be creative and include unconventional ingredients or techniques if they appear in the image. Include a list of ingredients with measurements and step-by-step cooking instructions."),
                new UserChatMessage($"Create a recipe for \"{foodName}\". The generated image was based on this prompt: \"{imagePrompt}\""));

            menuItem.Recipe = completion.Content[0].Text;
            return;
        }
        
        await using Stream imageStream = File.OpenRead(imagePath);
        BinaryData imageBytes = await BinaryData.FromStreamAsync(imageStream);

        {
            // Send the actual image to the AI for analysis
            ChatCompletion completion = await _chatClient.CompleteChatAsync(
                new SystemChatMessage(
                    "Your task is to create a detailed recipe for a dish based on the image provided. The recipe should match the content visible in the image, even if it seems impractical to actually make. Be creative and include unconventional ingredients or techniques if they appear in the image. Include a list of ingredients with measurements and step-by-step cooking instructions."),
                new UserChatMessage(
                    $"Create a recipe for \"{foodName}\". Analyze the attached image and create a recipe that matches what you see."),
                new UserChatMessage(ChatMessageContentPart.CreateImagePart(imageBytes, "image/png"), "The generated food image"));
            
            menuItem.Recipe = completion.Content[0].Text;
        }
    }
    
    private async Task<Image> GenerateImage(string prompt, string folderPath,
        CancellationToken cancellationToken = default)
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