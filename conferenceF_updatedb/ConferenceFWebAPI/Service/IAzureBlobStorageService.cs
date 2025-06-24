namespace ConferenceFWebAPI.Service
{
    public interface IAzureBlobStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string containerName);
        Task<bool> DeleteFileAsync(string filePath);
        // Task<byte[]> DownloadFileAsync(string filePath); // Có thể thêm nếu cần
    }
}
