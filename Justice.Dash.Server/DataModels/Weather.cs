namespace Justice.Dash.Server.DataModels;

public class Weather : BaseDataModel
{
    /// <summary>
    /// Indicates if it is currently raining
    /// </summary>
    public bool IsRaining { get; set; }
    
    /// <summary>
    /// Current rain measurement in mm/hour
    /// </summary>
    public double? RainAmount { get; set; }
    
    /// <summary>
    /// Current temperature in degrees Celsius
    /// </summary>
    public double? Temperature { get; set; }
    
    /// <summary>
    /// Current humidity percentage
    /// </summary>
    public double? Humidity { get; set; }
    
    /// <summary>
    /// Date and time when the data was last updated
    /// </summary>
    public DateTime LastUpdate { get; set; }
}