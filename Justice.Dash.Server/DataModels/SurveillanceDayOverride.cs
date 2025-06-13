using System.Text.Json.Serialization;

namespace Justice.Dash.Server.DataModels;

public class SurveillanceDayOverride : BaseDataModel
{
    public required SurveillanceType Type { get; set; }
    public required DateOnly Date { get; set; }
    public required string Responsible { get; set; }
    
    // Reference to the original weekly surveillance record (optional)
    public Guid? WeeklySurveillanceId { get; set; }
    
    [JsonIgnore]
    public Surveillance? WeeklySurveillance { get; set; }
}