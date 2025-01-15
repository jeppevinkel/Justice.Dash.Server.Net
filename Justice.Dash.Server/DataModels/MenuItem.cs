using System.Runtime.Serialization;

namespace Justice.Dash.Server.DataModels;

public class MenuItem : BaseDataModel
{
    public required DateOnly Date { get; set; }
    public required string Day { get; set; }
    public required int WeekNumber { get; set; }
    public required string FoodName { get; set; }
    public string? CorrectedFoodName { get; set; }
    public string? VeganizedFoodName { get; set; }
    public string? Description { get; set; }
    public string? VeganizedDescription { get; set; }
    public List<string> FoodContents { get; set; } = [];
    public Image? Image { get; set; }
    public Image? VeganizedImage { get; set; }
    public bool Dirty { get; set; } = true;
    public string? FoodModifier { get; set; }

    [IgnoreDataMember]
    public string FoodDisplayName => CorrectedFoodName ?? FoodName;
}