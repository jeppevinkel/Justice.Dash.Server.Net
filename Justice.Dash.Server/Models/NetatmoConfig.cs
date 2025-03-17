namespace Justice.Dash.Server.Models;

/// <summary>
/// Configuration model for Netatmo API
/// </summary>
public class NetatmoConfig
{
    /// <summary>
    /// Netatmo API Client ID
    /// </summary>
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>
    /// Netatmo API Client Secret
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
    
    /// <summary>
    /// Netatmo account username (email)
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Netatmo account password
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Refresh token for Netatmo API (if available)
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the Netatmo weather station to monitor
    /// </summary>
    public string StationId { get; set; } = string.Empty;
}