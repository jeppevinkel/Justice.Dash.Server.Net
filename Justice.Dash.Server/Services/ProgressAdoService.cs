using Justice.Dash.Server.DataModels;
using Microsoft.EntityFrameworkCore;

namespace Justice.Dash.Server.Services;

public class ProgressAdoService
{
    private readonly DashboardDbContext _dbContext;

    public ProgressAdoService(DashboardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProgressAdo?> GetProgressAsync()
    {
        return await _dbContext.ProgressAdo.FirstOrDefaultAsync();
    }

    public async Task<ProgressAdo?> GetGhProgressAsync()
    {
        return await _dbContext.ProgressAdo.FirstOrDefaultAsync(it => it.Id == Guid.Parse("59e00891-af5b-4e2d-b641-bc56fe78d17b"));
    }

    public async Task<ProgressAdo> UpdateProgressAsync(int completedItems, int totalItems)
    {
        var progress = await _dbContext.ProgressAdo.FirstOrDefaultAsync();
        
        if (progress == null)
        {
            progress = new ProgressAdo
            {
                CompletedItems = completedItems,
                TotalItems = totalItems
            };
            _dbContext.ProgressAdo.Add(progress);
        }
        else
        {
            progress.CompletedItems = completedItems;
            progress.TotalItems = totalItems;
        }

        await _dbContext.SaveChangesAsync();
        return progress;
    }

    public async Task<ProgressAdo> UpdateGhProgressAsync(int completedItems, int totalItems)
    {
        var progress = await _dbContext.ProgressAdo.FirstOrDefaultAsync(it => it.Id == Guid.Parse("59e00891-af5b-4e2d-b641-bc56fe78d17b"));
        
        if (progress == null)
        {
            progress = new ProgressAdo
            {
                Id = Guid.Parse("59e00891-af5b-4e2d-b641-bc56fe78d17b"),
                CompletedItems = completedItems,
                TotalItems = totalItems
            };
            _dbContext.ProgressAdo.Add(progress);
        }
        else
        {
            progress.CompletedItems = completedItems;
            progress.TotalItems = totalItems;
        }

        await _dbContext.SaveChangesAsync();
        return progress;
    }
}