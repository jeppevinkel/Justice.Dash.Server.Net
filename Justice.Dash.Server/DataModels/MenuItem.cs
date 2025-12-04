using System.Runtime.Serialization;

namespace Justice.Dash.Server.DataModels;

/// <summary>
/// Represents a menu item in the system, including its original and modified versions,
/// along with various states for content generation and modification.
/// </summary>
public class MenuItem : BaseDataModel
{
    /// <summary>
    /// The date this menu item is scheduled for
    /// </summary>
    public required DateOnly Date { get; set; }

    /// <summary>
    /// The day of the week this menu item is for
    /// </summary>
    public required string Day { get; set; }

    /// <summary>
    /// The week number in the year for this menu item
    /// </summary>
    public required int WeekNumber { get; set; }

    /// <summary>
    /// The original name of the food item
    /// </summary>
    public required string FoodName { get; set; }

    /// <summary>
    /// The corrected/normalized name of the food item
    /// </summary>
    public string? CorrectedFoodName { get; set; }

    /// <summary>
    /// The veganized version of the food name
    /// </summary>
    public string? VeganizedFoodName { get; set; }

    /// <summary>
    /// Description of the original food item
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Description of the veganized version
    /// </summary>
    public string? VeganizedDescription { get; set; }
    
    /// <summary>
    /// Recipe for the food item
    /// </summary>
    public string? Recipe { get; set; }

    /// <summary>
    /// List of ingredients or contents in the food item
    /// </summary>
    public List<string> FoodContents { get; set; } = [];

    /// <summary>
    /// The image of the original food item
    /// </summary>
    public Image? Image { get; set; }

    /// <summary>
    /// The image of the veganized version
    /// </summary>
    public Image? VeganizedImage { get; set; }

    /// <summary>
    /// Optional modifier affecting the food's presentation
    /// </summary>
    public FoodModifier? FoodModifier { get; set; }

    /// <summary>
    /// Indicates if the item has been manually modified
    /// </summary>
    public bool ManuallyModified { get; set; } = false;

    /// <summary>
    /// Flags indicating whether various aspects need to be regenerated or updated
    /// </summary>
    public bool NeedsNameCorrection { get; set; } = true;
    public bool NeedsVeganization { get; set; } = false;
    public bool NeedsDescription { get; set; } = true;
    public bool NeedsVeganDescription { get; set; } = false;
    public bool NeedsRecipeGeneration { get; set; } = true;
    public bool NeedsFoodContents { get; set; } = true;
    public bool NeedsImageRegeneration { get; set; } = true;
    public bool NeedsVeganImageRegeneration { get; set; } = false;

    /// <summary>
    /// Gets the display name for the food item, using the corrected name if available, otherwise the original name
    /// </summary>
    [IgnoreDataMember]
    public string FoodDisplayName => CorrectedFoodName ?? FoodName;
    
    /// <summary>
    /// Predefined list of food modifiers that can be applied to change the presentation of food items
    /// </summary>
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
        "The food is made of rainbows and sprinkles.",
        "The plating is weird.",
        "The plating is very traditional.",
        "The plating is upside down.",
        "The plating is like a fine dining restaurant.",
        "The plating is like a hearthy warm inn.",
    ];
}