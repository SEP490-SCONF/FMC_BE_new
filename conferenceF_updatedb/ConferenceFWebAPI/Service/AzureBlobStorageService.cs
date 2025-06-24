using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;

namespace ConferenceFWebAPI.Service
{
    public class AzureBlobStorageService : IAzureBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _paperContainerName;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration.GetValue<string>("AzureStorage:ConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("AzureStorage:ConnectionString is not configured.");
            }
            _blobServiceClient = new BlobServiceClient(connectionString);
            _paperContainerName = configuration.GetValue<string>("BlobContainers:Papers");
            if (string.IsNullOrEmpty(_paperContainerName))
            {
                throw new ArgumentNullException("BlobContainers:Papers is not configured.");
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string containerName)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File cannot be null or empty.");
            }

            // Lấy container client
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob); // Đảm bảo container tồn tại

            // Tạo tên file duy nhất để tránh trùng lặp
            // Ví dụ: Guid + original file extension
            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            // Lấy blob client
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            // Upload file
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            // Trả về URL công khai của file
            return blobClient.Uri.ToString();
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            try
            {
                // Phân tích URL để lấy tên container và tên blob
                Uri uri = new Uri(filePath);
                string containerName = uri.Segments[1].TrimEnd('/'); // Ví dụ: /papers/ -> papers
                string blobName = Path.GetFileName(uri.LocalPath); // Lấy tên file từ đường dẫn

                BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                return await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu có
                Console.WriteLine($"Error deleting blob: {ex.Message}");
                return false;
            }
        }
    }
}
