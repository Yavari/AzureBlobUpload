using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;

namespace Drone.Services
{
    public class AzureBlobSasToken
    {
        //https://learn.microsoft.com/en-us/azure/storage/blobs/sas-service-create-dotnet
        public static async Task<Uri> CreateServiceSASBlob(BlobClient blobClient, string? storedPolicyName = null)
        {
            // Check if BlobContainerClient object has been authorized with Shared Key
            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                BlobName = blobClient.Name,
                Resource = "b"
            };

            if (!blobClient.CanGenerateSasUri) 
                throw new Exception("Could not generate SAS token");

            if (storedPolicyName == null)
            {
                sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(3);
                sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);
            }
            else
            {
                sasBuilder.Identifier = storedPolicyName;
            }

            var sasURI = blobClient.GenerateSasUri(sasBuilder);
            return sasURI;

        }
    }
}
