using GooglePhotosAlbumFetch;
using GooglePhotosAlbumFetch.Models;
using Justice.Dash.Server.DataModels;
using Microsoft.EntityFrameworkCore;

namespace Justice.Dash.Server.Services;

public class DomicileService : BackgroundService
{
    private readonly ILogger<DomicileService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public DomicileService(ILogger<DomicileService> logger, IServiceProvider serviceProvider,
        IConfiguration configuration,
        IWebHostEnvironment env)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _env = env;
        _config = configuration;
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
            
            var images = await AlbumFetcher.FetchAlbumImages(
                _config.GetValue<string>("GooglePhotoAlbum") ?? throw new InvalidOperationException(),
                cancellationToken);

            foreach (ImageInfo image in images)
            {
                await DownloadImage(image, dbContext, cancellationToken);
            }
            
            await dbContext.SaveChangesAsync(cancellationToken);
            await Task.Delay(TimeSpan.FromHours(2), cancellationToken);
        }
    }

    private async Task DownloadImage(ImageInfo image, DashboardDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var basePath = Path.Combine(_env.ContentRootPath, "wwwroot");
        var imagePath = Path.Combine(basePath, "images", "domicile", $"{image.ImageUpdateDate}.png");
        Directory.CreateDirectory(Path.Combine(basePath, "images", "domicile"));
        using var httpClient = new HttpClient();

        Photo? photo =
            await dbContext.Photos.FirstOrDefaultAsync(it => it.Uid == image.Uid, cancellationToken: cancellationToken);

        if (photo is not null) return;
        
        var imageBytes = await httpClient.GetByteArrayAsync(image.LargeUrl, cancellationToken);
        await File.WriteAllBytesAsync(imagePath, imageBytes, cancellationToken);

        photo = new Photo
        {
            Uid = image.Uid,
            Height = image.Height,
            Width = image.Width,
            Path = Path.Combine("images", "domicile", $"{image.ImageUpdateDate}.png").Replace('\\', '/'),
            AlbumAddDate = image.AlbumAddDate,
            ImageUpdateDate = image.ImageUpdateDate,
        };
        
        await dbContext.Photos.AddAsync(photo, cancellationToken);
    }
}