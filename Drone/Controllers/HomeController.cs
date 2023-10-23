using System;
using System.Diagnostics;
using Azure.Storage;
using Azure.Storage.Blobs;
using Drone.Models;
using Drone.Options;
using Drone.Services;
using Drone.Services.AzureBlob;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Drone.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOptions<StorageAccountOptions> _storageAccountOptions;
        private readonly AzureAdClient _azureAdClient;
        private readonly AzureBlobClient _azureBlobClient;

        public HomeController(
            IOptions<StorageAccountOptions> storageAccountOptions, 
            AzureAdClient azureAdClient,
            AzureBlobClient azureBlobClient
        )
        {
            _storageAccountOptions = storageAccountOptions;
            _azureAdClient = azureAdClient;
            _azureBlobClient = azureBlobClient;
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

            viewModel.Videos.Add(new Uri("https://localhost:7035/video/Test.mp4"));
            foreach (var video in videos)
            {
                viewModel.Videos.Add(await GetUris(serviceUri, storageSharedKeyCredential, video));
            }

            return View(viewModel);
        }


        [HttpGet("/video/{**catchall}")]
        public async Task<IActionResult> Video(string catchall)
        {
            var url = catchall + HttpContext.Request.QueryString;
            var token = await _azureAdClient.GetToken();
            var result = await _azureBlobClient.GetVideoStream(url, HttpContext.Request.Headers.Single(x => x.Key == "Range").Value.Single(), token);
            // Should be using, how to handle?
            var contentRange = result.Content.Headers.Single(x => x.Key == "Content-Range");
            var acceptRanges = result.Headers.Single(x => x.Key == "Accept-Ranges");
            HttpContext.Response.StatusCode = 206;
            HttpContext.Response.Headers.Add("Content-Range", contentRange.Value.Single());
            HttpContext.Response.Headers.Add("Accept-Ranges", acceptRanges.Value.Single());
            return File(await result.Content.ReadAsStreamAsync(), "video/mp4", true);
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