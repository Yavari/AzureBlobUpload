using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Drone.Options;
using Microsoft.Extensions.Options;

namespace Drone.Services
{
    public class AzureBlobClient
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<StorageAccountOptions> _storageAccountOptions;

        public AzureBlobClient(
            HttpClient httpClient,
            IOptions<StorageAccountOptions> storageAccountOptions)
        {
            _httpClient = httpClient;
            _storageAccountOptions = storageAccountOptions;
        }
        public async Task<bool> PutBlock(string contentType, string path, string token, Stream stream, string id)
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            //https://stackoverflow.com/questions/67073448/azure-storage-rest-api-put-blob-api
            var rootPath = $"https://{_storageAccountOptions.Value.AccountName}.blob.core.windows.net/{_storageAccountOptions.Value.Container}/";
            using var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"{rootPath}{path}?comp=block&blockid={id}");
            var now = DateTime.UtcNow;
            requestMessage.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
            requestMessage.Headers.Add("x-ms-version", "2020-04-08");
            //httpRequestMessage.Headers.Add("x-ms-blob-type", "BlockBlob");
            //httpRequestMessage.Headers.Add("x-ms-blob-content-type", contentType);

            // Can this be changed to stream?
            requestMessage.Content = new ByteArrayContent(ms.ToArray());
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var httpResponseMessage = await _httpClient.SendAsync(requestMessage);
            if (httpResponseMessage.StatusCode == HttpStatusCode.Created)
                return true;

            var a = httpResponseMessage.StatusCode;
            var result = await httpResponseMessage.Content.ReadAsStringAsync();
            throw new Exception(result);
        }

        public async Task<string> GetBlockList(string path, string token)
        {
            var rootPath = $"https://{_storageAccountOptions.Value.AccountName}.blob.core.windows.net/{_storageAccountOptions.Value.Container}/";
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{rootPath}{path}?comp=blocklist&blocklisttype=all");
            requestMessage.Headers.Add("x-ms-version", "2020-04-08");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var httpResponseMessage = await _httpClient.SendAsync(requestMessage);
            var xml = await httpResponseMessage.Content.ReadAsStringAsync();
            return xml;
        }

        public async Task<string> ListBlobs(string token)
        {
            var rootPath = $"https://{_storageAccountOptions.Value.AccountName}.blob.core.windows.net/{_storageAccountOptions.Value.Container}?restype=container&comp=list&include=uncommittedblobs";
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, rootPath);
            requestMessage.Headers.Add("x-ms-version", "2020-04-08");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var httpResponseMessage = await _httpClient.SendAsync(requestMessage);
            var xml = await httpResponseMessage.Content.ReadAsStringAsync();
            return xml;
        }

        public async Task<bool> PutBlockList(string path, string token, IEnumerable<string> ids)
        {
            var rootPath = $"https://{_storageAccountOptions.Value.AccountName}.blob.core.windows.net/{_storageAccountOptions.Value.Container}/";
            using var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"{rootPath}{path}?comp=blocklist");
            requestMessage.Headers.Add("x-ms-version", "2020-04-08");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var xml = @"
<BlockList>
{0}
</BlockList>";

            var body = string.Format(xml, string.Join("\r\n", ids.Select(x => $"<Latest>{x}</Latest>")));
            requestMessage.Content = new StringContent(body, Encoding.UTF8, "text/xml");
            using var httpResponseMessage = await _httpClient.SendAsync(requestMessage);
            if (httpResponseMessage.StatusCode == HttpStatusCode.Created)
                return true;

            var a = httpResponseMessage.StatusCode;
            var result = await httpResponseMessage.Content.ReadAsStringAsync();
            return false;
        }
    }
}
