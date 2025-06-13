using Justice.Dash.Server.DataModels;

namespace Justice.Dash.Server.Models;

public class SurveillanceResponse
{
    public Surveillance WeeklySurveillance { get; set; } = null!;
    public List<SurveillanceDayOverride> DayOverrides { get; set; } = new();
}