namespace Drone.Options;

public class AdOptions
{
    public const string Position = "Ad";
    public string Tenant { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }

    public string AuthUrl => $"https://login.microsoftonline.com/{Tenant}/oauth2/token";
}