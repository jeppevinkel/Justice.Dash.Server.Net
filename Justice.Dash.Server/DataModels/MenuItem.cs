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
    public FoodModifier? FoodModifier { get; set; }
    public bool NeedsNameCorrection { get; set; } = true;
    public bool NeedsVeganization { get; set; } = true;
    public bool NeedsDescription { get; set; } = true;
    public bool NeedsVeganDescription { get; set; } = true;
    public bool NeedsFoodContents { get; set; } = true;
    public bool NeedsImageRegeneration { get; set; } = true;
    public bool NeedsVeganImageRegeneration { get; set; } = true;

    [IgnoreDataMember]
    public string FoodDisplayName => CorrectedFoodName ?? FoodName;
    
    public static readonly string[] FoodModifiers =
    [
        "The food is heavily colored blue.",
        "The food is heavily colored red.",
        "The food is heavily colored green.",
        "The food is heavily colored yellow.",
        "The food is heavily colored purple.",
        "The food is heavily colored white.",
        "The food is heavily colored black.",
        "The plate is presented in the style of Pixar.",
        "The plate is presented in the style of Dream Works.",
        "The plate is presented in the style of Disney.",
        "The plate is presented in the style of an old analog VHS film.",
        "The plate is presented in the style an image from the 18th century.",
        "The plate is presented in the middle of the night.",
        "The food is raw.",
        "The food is burnt.",
        "The food is from america.",
        "The food is from south america.",
        "The food is from africa.",
        "The food is from asia.",
        "The food is from antarctica.",
        "The food is from the far future.",
        "The food is from fantasy land.",
        "The food is from post apocalyptic future.",
        "The food is spread across the table.",
        "The food is served on a lush forest floor.",
        "The food is served deep down in the ocean.",
        "The food is served in space.",
        "The plating is weird.",
        "The plating is very traditional.",
        "The plating is upside down.",
        "The plating is like a fine dining restaurant.",
        "The plating is like a hearthy warm inn.",
    ];
}