using Justice.Dash.Server.DataModels;
using Microsoft.EntityFrameworkCore;

namespace Justice.Dash.Server;

public class DashboardDbContext(DbContextOptions<DashboardDbContext> options) : DbContext(options)
{
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Surveillance> Surveillance { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItem>().ToTable("menu_items").HasIndex(it => it.Date).IsUnique();
        modelBuilder.Entity<Image>().ToTable("images");
        modelBuilder.Entity<Surveillance>().ToTable("surveillance").HasIndex([
            nameof(DataModels.Surveillance.Type), nameof(DataModels.Surveillance.Week), nameof(DataModels.Surveillance.Year)
        ]).IsUnique();
    }
}