using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace ABC.Services.Storage
{
    public class BlobStorageService
    {
        //defined table client
        private readonly BlobContainerClient _blobContainerClient;

        //initialise the constructor
        public BlobStorageService(string storageConnectionString, string containerNamee)
        {
            var BlobServiceClient = new BlobServiceClient(storageConnectionString);
            _blobContainerClient = BlobServiceClient.GetBlobContainerClient(containerNamee);
            _blobContainerClient.CreateIfNotExists();
        }

        // Upload a new image and return the blob name
        public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
        {
            var blobClient = _blobContainerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream, overwrite: true);
            return fileName;
        }

        // Generate a temporary SAS URL (valid for 1 hour)
        public string GetImageSasUri(string blobName, int validMinutes = 60)
        {
            var blobClient = _blobContainerClient.GetBlobClient(blobName);

            if (!blobClient.CanGenerateSasUri)
                throw new InvalidOperationException("BlobClient cannot generate SAS URI. Make sure you use a credential that supports it.");

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _blobContainerClient.Name,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(validMinutes)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }

        // Delete an existing blob
        public async Task DeleteImageAsync(string blobName)
        {
            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
