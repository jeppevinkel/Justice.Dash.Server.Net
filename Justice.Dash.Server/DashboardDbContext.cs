using Justice.Dash.Server.DataModels;
using Microsoft.EntityFrameworkCore;

namespace Justice.Dash.Server;

/// <summary>
/// Database context for the Justice.Dash application.
/// Manages database interactions for menu items, images, photos, surveillance data, and food modifications.
/// </summary>
public class DashboardDbContext(DbContextOptions<DashboardDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the collection of menu items in the database
    /// </summary>
    public DbSet<MenuItem> MenuItems { get; set; }
    
    /// <summary>
    /// Gets or sets the collection of images in the database
    /// </summary>
    public DbSet<Image> Images { get; set; }
    
    /// <summary>
    /// Gets or sets the collection of photos in the database
    /// </summary>
    public DbSet<Photo> Photos { get; set; }
    
    /// <summary>
    /// Gets or sets the collection of surveillance data in the database
    /// </summary>
    public DbSet<Surveillance> Surveillance { get; set; }
    
    /// <summary>
    /// Gets or sets the collection of food modifiers in the database
    /// </summary>
    public DbSet<FoodModifier> FoodModifiers { get; set; }
    
    /// <summary>
    /// Gets or sets the collection of progress tracking data in the database
    /// </summary>
    public DbSet<ProgressAdo> ProgressAdo { get; set; }
    
    /// <summary>
    /// Gets or sets the collection of progress tracking data in the database
    /// </summary>
    public DbSet<Weather> Weather { get; set; }

    /// <summary>
    /// Configures the database model including table names, relationships, and indexes
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure the database schema</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItem>().Navigation(it => it.Image).AutoInclude();
        modelBuilder.Entity<MenuItem>().Navigation(it => it.VeganizedImage).AutoInclude();
        modelBuilder.Entity<MenuItem>().Navigation(it => it.FoodModifier).AutoInclude();
        modelBuilder.Entity<MenuItem>().ToTable("menu_items").HasIndex(it => it.Date).IsUnique();
        modelBuilder.Entity<Image>().ToTable("images");
        modelBuilder.Entity<Photo>().ToTable("photos").HasIndex(it => it.Uid);
        modelBuilder.Entity<Surveillance>().ToTable("surveillance").HasIndex([
            nameof(DataModels.Surveillance.Type), nameof(DataModels.Surveillance.Week), nameof(DataModels.Surveillance.Year)
        ]).IsUnique();
        modelBuilder.Entity<FoodModifier>().ToTable("food_modifiers");
        modelBuilder.Entity<ProgressAdo>().ToTable("progress_ado");
        modelBuilder.Entity<Weather>().ToTable("weather");
    }
}