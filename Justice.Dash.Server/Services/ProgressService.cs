using Justice.Dash.Server.DataModels;
using Microsoft.EntityFrameworkCore;

namespace Justice.Dash.Server.Services;

public class ProgressService
{
    private readonly DashboardDbContext _dbContext;

    public ProgressService(DashboardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Progress?> GetProgressAsync()
    {
        return await _dbContext.Progress.FirstOrDefaultAsync();
    }

    public async Task<Progress> UpdateProgressAsync(int completedItems, int totalItems)
    {
        var progress = await _dbContext.Progress.FirstOrDefaultAsync();
        
        if (progress == null)
        {
            progress = new Progress
            {
                CompletedItems = completedItems,
                TotalItems = totalItems
            };
            _dbContext.Progress.Add(progress);
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