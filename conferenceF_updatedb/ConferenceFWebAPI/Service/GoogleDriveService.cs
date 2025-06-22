using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

namespace ConferenceFWebAPI.Service
{
    public class GoogleDriveService
    {
        private readonly DriveService _driveService;
        private readonly string _folderId = "1-lMyRVFOfC2pXToLOg_3eahAZFS6QLpW"; // 👈 ID của thư mục Drive đã chia sẻ

        public GoogleDriveService(IWebHostEnvironment env)
        {
            var path = Path.Combine(env.ContentRootPath, "credentials.json");
            GoogleCredential credential;

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(DriveService.Scope.Drive);
            }

            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Conference Upload"
            });
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = file.FileName,
                Parents = new List<string> { _folderId }
            };

            using var stream = file.OpenReadStream();
            var request = _driveService.Files.Create(fileMetadata, stream, file.ContentType);
            request.Fields = "id";

            await request.UploadAsync();

            var fileId = request.ResponseBody.Id;
            return $"https://drive.google.com/uc?id={fileId}&export=download"; // 👈 Link để tải
        }
    }
}
