namespace Justice.Dash.Server.Models;

public class MenuItemUpdate
{
    public string? FoodName { get; set; }
    public string? CorrectedFoodName { get; set; }
    public string? VeganizedFoodName { get; set; }
    public string? Description { get; set; }
    public string? VeganizedDescription { get; set; }
    public Guid? FoodModifierId { get; set; }
    public bool RegenerateImages { get; set; }
    public bool RegenerateDescriptions { get; set; }
    public bool RegenerateNames { get; set; }
    public bool RegenerateFoodContents { get; set; }
}