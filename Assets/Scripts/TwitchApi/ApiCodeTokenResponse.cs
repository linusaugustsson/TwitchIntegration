using System;

/// Data object to parse API response for auth token into
[Serializable]
public class ApiCodeTokenResponse
{
    public string access_token;
    public int expires_in;
    public string refresh_token;
    public string[] scope;
    public string token_type;
}
