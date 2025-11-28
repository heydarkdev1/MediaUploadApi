using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;

namespace MediaUploadApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IConfiguration _config;

        public UploadController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        [Route("image")]
        [RequestSizeLimit(10_000_000)] // 10 MB
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            string containerName = _config["BlobSettings:ContainerName"]!;
            string connectionString = _config["BlobConnectionString"]!;

            if (string.IsNullOrEmpty(connectionString))
            {
                return StatusCode(500, "Blob connection string not configured.");
            }

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync();

            string blobName = $"{Guid.NewGuid()}-{file.FileName}";
            var blobClient = containerClient.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            return Ok(new
            {
                message = "Image uploaded successfully",
                blobUrl = blobClient.Uri.ToString()
            });
        }
    }
}
