namespace Justice.Dash.Server.DataModels;

public class MenuItem : BaseDataModel
{
    public required DateOnly Date { get; set; }
    public required string Day { get; set; }
    public required int WeekNumber { get; set; }
    public required string FoodName { get; set; }
    public string? CorrectedFoodName { get; set; }
    public string? Description { get; set; }
    public List<string> FoodContents { get; set; } = [];
    public Image? Image { get; set; }
    public bool Dirty { get; set; } = true;
}