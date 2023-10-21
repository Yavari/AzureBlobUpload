using Drone.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Drone.Controllers
{
    public class BlobController : Controller
    {
        private record Chunk(long Start, long End, long TotalSize);
        private readonly PoorMansDb _poorMansDb;
        private readonly AzureAdClient _azureAdClient;
        private readonly AzureBlobClient _azureBlobClient;

        public BlobController(
            PoorMansDb poorMansDb,
            AzureAdClient azureAdClient,
            AzureBlobClient azureBlobClient)
        {
            _poorMansDb = poorMansDb;
            _azureAdClient = azureAdClient;
            _azureBlobClient = azureBlobClient;
        }
        public IActionResult Index() => View();
        public async Task<IActionResult> List() => Xml(await _azureBlobClient.ListBlobs(await _azureAdClient.GetToken()));
        public async Task<IActionResult> BlockList(string filename) => Xml(await _azureBlobClient.GetBlockList(filename, await _azureAdClient.GetToken()));


        [HttpPut("/upload/{filename}")]
        public async Task<IActionResult> Upload(string filename)
        {
            var token = await _azureAdClient.GetToken();
            var contentLength = HttpContext.Request.Headers.ContentLength;
            var contentRange = GetContentRange(HttpContext.Request.Headers["Content-Range"].Single());

            if (contentRange.Start == 0)
            {
                //Filename = $"{RandomString(10)}.txt";
                _poorMansDb.Db.Clear();
            }

            var id = Base64UrlEncoder.Encode(Guid.NewGuid().ToString());
            _poorMansDb.Db.Enqueue(id);
            var contentType = HttpContext.Request.Headers.ContentType.Single();
            var result = await _azureBlobClient.PutBlock(contentType, filename, token, HttpContext.Request.Body, id);

            if (contentRange.End + 1 == contentRange.TotalSize)
            {
                await _azureBlobClient.PutBlockList(filename, token, GetIds());
            }

            return Ok();
        }

        private static IActionResult Xml(string blobs) => new ContentResult { Content = blobs, ContentType = "application/xml", StatusCode = 200 };

        private IEnumerable<string> GetIds()
        {
            while (_poorMansDb.Db.Any())
            {
                yield return _poorMansDb.Db.Dequeue();
            }
        }

        private Chunk GetContentRange(string contentRange)
        {
            if (!contentRange.StartsWith("bytes "))
                throw new Exception($"Wrong format {contentRange}");

            contentRange = contentRange.Remove(0, 6);
            var a = contentRange.Split('-');
            var b = a[1].Split('/');
            return new Chunk(long.Parse(a[0]), long.Parse(b[0]), long.Parse(b[1]));
        }
    }
}
