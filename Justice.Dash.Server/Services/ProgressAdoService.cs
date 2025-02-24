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
}