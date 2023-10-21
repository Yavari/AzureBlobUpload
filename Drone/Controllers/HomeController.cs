using System.Diagnostics;
using Azure.Storage;
using Azure.Storage.Blobs;
using Drone.Models;
using Drone.Options;
using Drone.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Drone.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOptions<StorageAccountOptions> _storageAccountOptions;

        public HomeController(IOptions<StorageAccountOptions> storageAccountOptions)
        {
            _storageAccountOptions = storageAccountOptions;
        }

        public IActionResult Index() => RedirectToAction("Index", "Blob");

        public async Task<IActionResult> Play()
        {
            var accountName = _storageAccountOptions.Value.AccountName;
            var accountKey = _storageAccountOptions.Value.AccountKey;
            var serviceUri = new Uri($"https://{accountName}.blob.core.windows.net");
            var storageSharedKeyCredential = new StorageSharedKeyCredential(accountName, accountKey);
            var videos = new[] { "Test.mp4" };
            var viewModel = new VideoViewModel { Videos = new List<Uri>() };

            foreach (var video in videos)
            {
                viewModel.Videos.Add(await GetUris(serviceUri, storageSharedKeyCredential, video));
            }

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<Uri> GetUris(Uri serviceUri, StorageSharedKeyCredential storageSharedKeyCredential, string filename)
        {
            // var credential = new ClientSecretCredential(_adOptions.Value.Tenant, _adOptions.Value.ClientId, _adOptions.Value.ClientSecret);
            var blobServiceClient = new BlobServiceClient(serviceUri, storageSharedKeyCredential);

            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_storageAccountOptions.Value.Container);
            var blobClient = blobContainerClient.GetBlobClient(filename);
            return await AzureBlobSasToken.CreateServiceSASBlob(blobClient);
        }
    }
}