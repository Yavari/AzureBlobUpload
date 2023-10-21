using Drone.Models;
using Drone.Options;
using Microsoft.Extensions.Options;

namespace Drone.Services
{
    public class AzureAdClient
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<AdOptions> _adOptions;

        public AzureAdClient(
            HttpClient httpClient,
            IOptions<AdOptions> adOptions)
        {
            _httpClient = httpClient;
            _adOptions = adOptions;
        }

        public async Task<string> GetToken()
        {
            var content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("Grant_type", "client_credentials"),
                new("Client_id", _adOptions.Value.ClientId),
                new("Client_secret", _adOptions.Value.ClientSecret),
                new("resource", "https://storage.azure.com")
            });

            using var req = new HttpRequestMessage(HttpMethod.Post, string.Format(_adOptions.Value.AuthUrl)) { Content = content };
            using var res = await _httpClient.SendAsync(req);
            var result = await res.Content.ReadFromJsonAsync<TokenResult>();
            return result.access_token;
        }
    }
}
