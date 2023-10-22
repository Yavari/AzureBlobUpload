using Drone.Services;
using Drone.Services.AzureBlob;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Drone.Controllers
{
    public class BlobController : Controller
    {
        private static HashSet<char> s_Allowed = new(@"1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz.");
        private readonly ILogger<BlobController> _logger;
        private readonly PoorMansDb _poorMansDb;
        private readonly AzureAdClient _azureAdClient;
        private readonly AzureBlobClient _azureBlobClient;

        public BlobController(
            ILogger<BlobController> logger,
            PoorMansDb poorMansDb,
            AzureAdClient azureAdClient,
            AzureBlobClient azureBlobClient)
        {
            _logger = logger;
            _poorMansDb = poorMansDb;
            _azureAdClient = azureAdClient;
            _azureBlobClient = azureBlobClient;
        }
        public IActionResult Index() => View();
        public async Task<IActionResult> List() => Xml(await _azureBlobClient.ListBlobs(await _azureAdClient.GetToken()));
        public async Task<IActionResult> BlockList(string filename) => Xml(await _azureBlobClient.GetBlockList(filename, await _azureAdClient.GetToken()));
        public async Task<IActionResult> Delete(string filename) => Content((await _azureBlobClient.DeleteBlob(filename, await _azureAdClient.GetToken())).ToString());
        public async Task<IActionResult> DeleteUncommitted()
        {
            await _azureBlobClient.DeleteUncommittedBlobs(await _azureAdClient.GetToken());
            return Ok();
        }

        [HttpPut("/upload/{filename}")]
        public async Task<IActionResult> Upload(string filename)
        {
            _logger.LogInformation("Start upload");
            filename = SafeFileName(filename);
            var token = await _azureAdClient.GetToken();
            var contentLength = HttpContext.Request.Headers.ContentLength;
            var chunk = GetContentRange(HttpContext.Request.Headers["Content-Range"].Single());

            if (chunk.Start == 0)
            {
                if (_poorMansDb.Db.TryGetValue(filename, out var value))
                {
                    value.Clear();
                }
                else
                {
                    _poorMansDb.Db[filename] = new List<PoorMansDb.MetaData>();
                }

            }

            var contentType = HttpContext.Request.Headers.ContentType.Single();
            var ids = await _azureBlobClient.PutBlock(contentType, filename, token, HttpContext.Request.Body, chunk.Size);
            _poorMansDb.Db[filename].Add(new PoorMansDb.MetaData(chunk, ids.ToArray()));
            if (_poorMansDb.Db[filename].Sum(x => x.Chunk.Size) == chunk.TotalSize)
            {
                var hashes = _poorMansDb.Db[filename].OrderBy(x => x.Chunk.Start).Select(x => x.Hashes).SelectMany(x => x);
                await _azureBlobClient.PutBlockList(filename, token, hashes);
            }

            _logger.LogInformation("End upload");
            return Ok();
        }

        private static IActionResult Xml(string blobs) => new ContentResult { Content = blobs, ContentType = "application/xml", StatusCode = 200 };

        private PoorMansDb.Chunk GetContentRange(string contentRange)
        {
            if (!contentRange.StartsWith("bytes "))
                throw new Exception($"Wrong format {contentRange}");

            contentRange = contentRange.Remove(0, 6);
            var a = contentRange.Split('-');
            var b = a[1].Split('/');
            var start = long.Parse(a[0]);
            var end = long.Parse(b[0]);
            return new PoorMansDb.Chunk(start, end, end - start + 1, long.Parse(b[1]));
        }


        private string SafeFileName(string filename) => string.Concat(filename.Where(c => s_Allowed.Contains(c)));
    }
}
