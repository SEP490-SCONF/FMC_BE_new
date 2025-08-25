using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;

namespace ConferenceFWebAPI.Service
{
    public class AzureBlobStorageService : IAzureBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _paperContainerName;
        private readonly string _proceedingsContainerName; // Thêm trường này
        private readonly IConfiguration _configuration; // Giữ lại trường này

        public AzureBlobStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
            var connectionString = _configuration.GetValue<string>("AzureStorage:ConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("AzureStorage:ConnectionString is not configured.");
            }
            _blobServiceClient = new BlobServiceClient(connectionString);
            _paperContainerName = _configuration.GetValue<string>("BlobContainers:Papers") ?? "papers";
            _proceedingsContainerName = _configuration.GetValue<string>("BlobContainers:Proceedings") ?? "proceedings"; // Gán giá trị ở đây
        }

        public async Task<string> UploadProceedingAsync(Stream fileStream, string fileName)
        {
            var proceedingsContainerName = _configuration.GetValue<string>("BlobContainers:Proceedings") ?? "proceedings";

            if (string.IsNullOrEmpty(proceedingsContainerName))
            {
                throw new ArgumentNullException("BlobContainers:Proceedings is not configured.");
            }

            // Lấy đối tượng container client
            var containerClient = _blobServiceClient.GetBlobContainerClient(proceedingsContainerName);

            // Tạo container nếu chưa tồn tại
            await containerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);

            // Lấy đối tượng blob client
            var blobClient = containerClient.GetBlobClient(fileName);

            // Đặt lại vị trí luồng về đầu nếu cần
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            // Tải luồng lên blob, ghi đè nếu đã tồn tại
            await blobClient.UploadAsync(fileStream, overwrite: true);

            // Trả về URL tuyệt đối của blob
            return blobClient.Uri.ToString();
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

        public async Task<string> UploadContentAsync(string content, string fileName, string containerName, string contentType)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentException("Content cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty.");
            }

            // Lấy container client
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob); // Đảm bảo container tồn tại

            // Lấy blob client
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            // Convert content to byte array
            byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(content);

            // Upload content
            using (var stream = new MemoryStream(contentBytes))
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });
            }

            // Trả về URL công khai của file
            return blobClient.Uri.ToString();
        }

        public async Task<string> UploadImageAsync(byte[] imageBytes, string fileName, string containerName)
        {
            if (imageBytes == null || imageBytes.Length == 0)
            {
                throw new ArgumentException("Image bytes cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty.");
            }

            // Lấy container client
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob); // Đảm bảo container tồn tại

            // Lấy blob client
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            // Upload image bytes
            using (var stream = new MemoryStream(imageBytes))
            {
                var contentType = fileName.EndsWith(".svg") ? "image/svg+xml" : "image/png";
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });
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

        public async Task<Stream> DownloadFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                throw new ArgumentException("File URL cannot be null or empty.");

            Uri uri = new Uri(fileUrl);
            string containerName = uri.Segments[1].TrimEnd('/');
            string blobName = string.Join("", uri.Segments.Skip(2));

            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            var downloadInfo = await blobClient.DownloadAsync();
            MemoryStream ms = new MemoryStream();
            await downloadInfo.Value.Content.CopyToAsync(ms);
            ms.Position = 0; // reset về đầu stream
            return ms;
        }


        public async Task<string> UploadStreamAsync(Stream stream, string fileName, string containerName, string contentType)
        {
            if (stream == null || stream.Length == 0)
                throw new ArgumentException("Stream cannot be null or empty.");
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be null or empty.");

            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            stream.Position = 0; // reset
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });

            return blobClient.Uri.ToString();
        }


    }
}
