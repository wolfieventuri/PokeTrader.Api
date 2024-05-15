using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace PokeTrader.Api
{
    public class UploadBlobImage
    {
        private readonly ILogger<UploadBlobImage> _logger;
        private readonly BlobServiceClient _blobServiceClient;

        public UploadBlobImage(ILogger<UploadBlobImage> logger, BlobServiceClient blobServiceClient)
        {
            _logger = logger;
            _blobServiceClient = blobServiceClient;
        }

        [Function("UploadBlobImage")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string localPath = "charmander.png"; 
            byte[] fileData = File.ReadAllBytes(localPath);
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient("pokemon-images");

            var blobName = $"charmander-{Guid.NewGuid().ToString().Substring(0, 4)}";
            var blobClient = blobContainerClient.GetBlobClient(blobName);


            _logger.LogInformation("C# HTTP trigger function processed a request.");


            try
            {
                // Open the file stream with asynchronous flag and dispose of it correctly after use
                using (FileStream stream = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
                {
                    BlobUploadOptions options = new BlobUploadOptions
                    {
                        
                        HttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = "image/png"  // Explicitly set the content type

                        }
                    };

                    // Asynchronously upload the blob with headers
                    var resUpload = await blobClient.UploadAsync(stream, options);
                    _logger.LogInformation($"Uploaded {blobName} successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading blob: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return new OkObjectResult("Image uploaded successfully!");

            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
