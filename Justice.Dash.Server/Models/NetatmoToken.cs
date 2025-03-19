using Netatmo.Models;

namespace Justice.Dash.Server.Models;

public class NetatmoToken
{
    public NetatmoToken()
    {
        
    }

    public NetatmoToken(CredentialToken credentialToken)
    {
        AccessToken = credentialToken.AccessToken;
        RefreshToken = credentialToken.RefreshToken;
    }

    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}