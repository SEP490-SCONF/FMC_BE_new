namespace ConferenceFWebAPI.Service
{
    public interface IAzureBlobStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string containerName);
        Task<string> UploadContentAsync(string content, string fileName, string containerName, string contentType);
        Task<string> UploadImageAsync(byte[] imageBytes, string fileName, string containerName);
        Task<bool> DeleteFileAsync(string filePath);
        // Task<byte[]> DownloadFileAsync(string filePath); // Có thể thêm nếu cần
        Task<Stream> DownloadFileAsync(string fileUrl);
        Task<string> UploadStreamAsync(Stream stream, string fileName, string containerName, string contentType);
        Task<string> UploadProceedingAsync(Stream fileStream, string fileName);

    }
}
