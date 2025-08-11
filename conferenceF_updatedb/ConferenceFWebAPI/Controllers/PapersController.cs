using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.Paper;
using ConferenceFWebAPI.DTOs.Papers;
using ConferenceFWebAPI.Service;
using DataAccess;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Repository;
using System.Security.Claims;
using System.Text.RegularExpressions;
using ConferenceFWebAPI.Services.PdfTextExtraction;

namespace ConferenceFWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PapersController : ControllerBase
    {
        private readonly IPaperRepository _paperRepository;
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IPaperRevisionRepository _paperRevisionRepository;
        private readonly IUserConferenceRoleRepository _userConferenceRoleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConferenceRepository _conferenceRepository;
        private readonly IConferenceRoleRepository _conferenceRoleRepository;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;
        private readonly PaperDeadlineService _paperDeadlineService; // THÊM CÁI NÀY
        private readonly ITimeLineRepository _timeLineRepository;
        private readonly IAiSpellCheckService _aiSpellCheckService;
        private readonly INotificationRepository _notificationRepository; // Add this repository
                                                                          // If you are using SignalR, also inject the hub context:
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly DeepLTranslationService _translationService;
        private readonly PdfService _pdfService; // Tiêm PdfService vào đây


        public PapersController(
            IPaperRepository paperRepository,
            IAzureBlobStorageService azureBlobStorageService,
            IConfiguration configuration,
            IMapper mapper,
            IPaperRevisionRepository paperRevisionRepository,
            IUserConferenceRoleRepository userConferenceRoleRepository,
            IUserRepository userRepository,
            IConferenceRepository conferenceRepository,
            IConferenceRoleRepository conferenceRoleRepository,
            IEmailService emailService,
            IWebHostEnvironment env,
            PaperDeadlineService paperDeadlineService,
            IAiSpellCheckService aiSpellCheckService,// THÊM VÀO CONSTRUCTOR
            ITimeLineRepository timeLineRepository
            INotificationRepository notificationRepository,
             IHubContext<NotificationHub> hubContext,
             DeepLTranslationService translationService,
             PdfService pdfService)

        {
            _paperRepository = paperRepository;
            _azureBlobStorageService = azureBlobStorageService;
            _configuration = configuration;
            _mapper = mapper;
            _paperRevisionRepository = paperRevisionRepository;
            _userConferenceRoleRepository = userConferenceRoleRepository;
            _userRepository = userRepository;
            _conferenceRepository = conferenceRepository;
            _conferenceRoleRepository = conferenceRoleRepository;
            _emailService = emailService;
            _env = env;
            _paperDeadlineService = paperDeadlineService; // GÁN
            _timeLineRepository = timeLineRepository; // GÁN
            _aiSpellCheckService = aiSpellCheckService;
             _notificationRepository = notificationRepository;
            _hubContext = hubContext;
            _translationService = translationService;
            _pdfService = pdfService;

        }

        [HttpGet("conference/{conferenceId}")]
        [ProducesResponseType(typeof(List<PaperResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetPapersByConference(int conferenceId)
        {
            var papers = _paperRepository.GetPapersByConferenceId(conferenceId);
            if (papers == null || !papers.Any())
                return NotFound($"No papers found for conference ID: {conferenceId}");

            var paperDto = _mapper.Map<List<PaperResponseDto>>(papers);
            return Ok(paperDto);
        }
        [HttpGet("conference/{conferenceId}/status/submitted")]
        [ProducesResponseType(typeof(List<PaperResponseWT>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetPapersByConferenceAndStatusSubmitted(int conferenceId)
        {
            var papers = _paperRepository.GetPapersByConferenceIdAndStatus(conferenceId, "Submitted");
            if (papers == null || !papers.Any())
                return NotFound($"No submitted papers found for conference ID: {conferenceId}");

            var paperDto = _mapper.Map<List<PaperResponseWT>>(papers);
            return Ok(paperDto);
        }


        [HttpPost("upload-pdf")]
        public async Task<IActionResult> UploadPdf([FromForm] PaperUploadDto paperDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (paperDto.PdfFile == null || paperDto.PdfFile.Length == 0)
                return BadRequest("No file uploaded.");

            if (paperDto.AuthorIds == null || !paperDto.AuthorIds.Any())
                return BadRequest("At least one author must be provided.");

            int uploaderUserId;

            if (_env.IsDevelopment())
            {
                uploaderUserId = paperDto.AuthorIds.First();
                Console.WriteLine($"[DEVELOPMENT MODE] Inferred UploaderUserId: {uploaderUserId}");
            }
            else
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out uploaderUserId))
                    return Unauthorized("User is not authenticated or user ID is not available.");
            }

            if (!paperDto.AuthorIds.Contains(uploaderUserId))
                return BadRequest($"User ID {uploaderUserId} must be one of the provided Author IDs.");

            try
            {
                var paperContainerName = _configuration.GetValue<string>("BlobContainers:Papers");
                if (string.IsNullOrEmpty(paperContainerName))
                    return StatusCode(500, "Blob storage container name is not configured.");

                string fileUrl = await _azureBlobStorageService.UploadFileAsync(paperDto.PdfFile, paperContainerName);

                var paper = _mapper.Map<Paper>(paperDto);
                paper.FilePath = fileUrl;
                paper.SubmitDate = DateTime.UtcNow;
                paper.Status = "Submitted";
                paper.IsPublished = false;

                paper.PaperAuthors = paperDto.AuthorIds
                    .Distinct()
                    .Select((id, index) => new PaperAuthor
                    {
                        AuthorId = id,
                        AuthorOrder = index + 1
                    }).ToList();

                await _paperRepository.AddPaperAsync(paper);

                // THÊM LỜI GỌI ĐẾN PAPERDEADLINESERVICE NGAY SAU KHI LƯU BÀI BÁO THÀNH CÔNG
                // paper.PaperId đã có giá trị sau khi AddPaperAsync
                await _paperDeadlineService.SchedulePaperReminders(paper.PaperId);


                var initialRevision = new PaperRevision
                {
                    PaperId = paper.PaperId,
                    FilePath = fileUrl,
                    Status = "Submitted",
                    SubmittedAt = DateTime.UtcNow
                };
                await _paperRevisionRepository.AddPaperRevisionAsync(initialRevision);

                int conferenceId = paper.ConferenceId;
                int newRoleId = 2; // Role = 2

                var conference = await _conferenceRepository.GetById(conferenceId);
                var newRole = await _conferenceRoleRepository.GetById(newRoleId);

                 var uploaderUser = await _userRepository.GetById(uploaderUserId);
                if (uploaderUser != null)
                {
                    string notificationTitle = "Nộp bài thành công!";
                    string notificationContent = $"Bạn đã nộp bài báo '{paper.Title}' thành công cho hội thảo '{conference.Title}'.";

                    // Tạo và lưu thông báo vào cơ sở dữ liệu
                    var notification = new Notification
                    {
                        Title = notificationTitle,
                        Content = notificationContent,
                        UserId = uploaderUserId,
                        RoleTarget = "Author",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _notificationRepository.AddNotificationAsync(notification);

                    // Gửi thông báo real-time qua SignalR
                    await _hubContext.Clients.User(uploaderUserId.ToString()).SendAsync("ReceiveNotification", notificationTitle, notificationContent);

                // --- Logic cập nhật vai trò và gửi email cho từng tác giả ---
                if (conference != null && newRole != null)
                {
                    foreach (var authorId in paperDto.AuthorIds.Distinct())
                    {
                        var authorUser = await _userRepository.GetById(authorId);
                        if (authorUser == null)
                        {
                            Console.WriteLine($"Warning: Author User ID {authorId} not found. Skipping role update and email for this author.");
                            continue;
                        }

                        var updatedRoleAssignment = await _userConferenceRoleRepository
                            .UpdateConferenceRoleForUserInConference(authorId, conferenceId, newRoleId);

                        string emailSubject;
                        string emailBody;

                        if (updatedRoleAssignment == null)
                        {
                            var newAssignment = new UserConferenceRole
                            {
                                UserId = authorId,
                                ConferenceId = conferenceId,
                                ConferenceRoleId = newRoleId,
                                AssignedAt = DateTime.UtcNow
                            };
                            await _userConferenceRoleRepository.Add(newAssignment);
                            Console.WriteLine($"Created new UserConferenceRole for User {authorId} in Conference {conferenceId} with Role {newRole.RoleName}.");

                            emailSubject = $"Vai trò mới của bạn trong hội thảo '{conference.Title}'";
                            emailBody = $@"
                                <h3>Xin chào {authorUser.Name},</h3>
                                <p>Bạn vừa được gán vai trò <strong>{newRole.RoleName}</strong> trong hội thảo <strong>{conference.Title}</strong> vì đã tải lên bài báo.</p>
                                <p>Thời gian gán: {DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm")} UTC</p>
                                <p>Vui lòng đăng nhập hệ thống để theo dõi thông tin chi tiết.</p>
                                <br/>
                                <p>Trân trọng,<br/>Ban tổ chức</p>";
                        }
                        else if (updatedRoleAssignment.ConferenceRoleId != newRoleId)
                        {
                            Console.WriteLine($"Updated UserConferenceRole for User {authorId} in Conference {conferenceId} to Role {newRole.RoleName}.");

                            emailSubject = $"Cập nhật vai trò của bạn trong hội thảo '{conference.Title}'";
                            emailBody = $@"
                                <h3>Xin chào {authorUser.Name},</h3>
                                <p>Vai trò của bạn trong hội thảo <strong>{conference.Title}</strong> đã được cập nhật thành <strong>{newRole.RoleName}</strong> do bạn đã tải lên bài báo.</p>
                                <p>Thời gian cập nhật: {DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm")} UTC</p>
                                <p>Vui lòng đăng nhập hệ thống để theo dõi thông tin chi tiết.</p>
                                <br/>
                                <p>Trân trọng,<br/>Ban tổ chức</p>";
                        }
                        else
                        {
                            Console.WriteLine($"User {authorId} already has Role {newRole.RoleName} in Conference {conferenceId}. No email sent.");
                            continue;
                        }

                        await _emailService.SendEmailAsync(authorUser.Email, emailSubject, emailBody);
                    }
                }
                else
                {
                    Console.WriteLine($"Warning: Missing Conference ({conferenceId}) or New Role ({newRoleId}) details. Skipping role updates and emails for authors.");
                }

                return Ok(new
                {
                    Message = "File uploaded and paper + revision saved. Roles updated and reminders scheduled.",
                    FileUrl = fileUrl,
                    PaperId = paper.PaperId,
                    RevisionId = initialRevision.RevisionId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Upload error: {ex.ToString()}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            var papersQuery = _paperRepository.GetAllPapers();
            if (papersQuery == null) return NotFound("No papers found.");

            var paperDtos = _mapper.ProjectTo<PaperResponseDto>(papersQuery);
            if (!paperDtos.Any()) return NotFound("No active papers found.");

            return Ok(paperDtos);
        }

        [HttpGet("{key}")]
        [EnableQuery]
        public async Task<IActionResult> Get([FromRoute] int key)
        {
            var paper = await _paperRepository.GetPaperByIdAsync(key);
            if (paper == null) return NotFound($"Paper with ID {key} not found.");

            var paperDto = _mapper.Map<PaperResponseDto>(paper);
            return Ok(paperDto);
        }

        [HttpPut("mark-as-deleted/{paperId}")]
        public async Task<IActionResult> MarkPaperAsDeleted(int paperId)
        {
            // Để hủy job Hangfire, chúng ta cần truy vấn Paper với thông tin Conference và TimeLines
            // Điều này yêu cầu một phương thức repository mới nếu chưa có, ví dụ: GetPaperWithConferenceAndTimelinesAsync
            var paper = await _paperRepository.GetPaperWithConferenceAndTimelinesAsync(paperId);
            if (paper == null) return NotFound("Paper not found.");

            try
            {
                paper.Status = "Deleted";
                await _paperRepository.UpdatePaperAsync(paper);

                // THÊM: Hủy các job Hangfire liên quan đến các timeline của hội thảo này
                // nếu bài báo bị xóa (hoặc "soft-deleted")
                if (paper.Conference != null && paper.Conference.TimeLines != null)
                {
                    foreach (var timeline in paper.Conference.TimeLines)
                    {
                        if (!string.IsNullOrEmpty(timeline.HangfireJobId))
                        {
                            timeline.HangfireJobId = null;
                            await _timeLineRepository.UpdateTimeLineAsync(timeline); // Cập nhật TimeLine trong DB
                        }
                    }
                }

                return Ok($"Paper with ID {paperId} marked as 'Deleted' and associated reminders cancelled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking paper as deleted: {ex.ToString()}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{paperId}/publish")]
        public async Task<IActionResult> UpdatePaperPublishStatus(int paperId, [FromBody] PaperPublishDto publishDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var paper = await _paperRepository.GetPaperByIdAsync(paperId);
            if (paper == null) return NotFound($"Paper with ID {paperId} not found.");

            try
            {
                paper.IsPublished = publishDto.IsPublished;
                await _paperRepository.UpdatePaperAsync(paper);
                return Ok($"Paper {paperId} publish status updated to {publishDto.IsPublished}.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}/conference/{conferenceId}")]
        [ProducesResponseType(typeof(List<PaperResponseWT>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetPapersByUserAndConference(int userId, int conferenceId)
        {
            if (userId <= 0 || conferenceId <= 0)
                return BadRequest("User ID and Conference ID must be positive.");

            var papers = _paperRepository.GetPapersByUserIdAndConferenceId(userId, conferenceId);
            if (papers == null || !papers.Any())
                return NotFound($"No papers for User ID {userId} in Conference ID {conferenceId}.");

            var paperDtos = _mapper.Map<List<PaperResponseWT>>(papers);
            return Ok(paperDtos);
        }

        [HttpGet("conference/{conferenceId}/published")]
        [ProducesResponseType(typeof(List<PaperResponseWT>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetPublishedPapers(int conferenceId)
        {
            var papers = _paperRepository.GetPublishedPapersByConferenceId(conferenceId);
            if (papers == null || !papers.Any())
                return NotFound($"No published papers found for conference ID: {conferenceId}");

            var paperDtos = _mapper.Map<List<PaperResponseWT>>(papers);
            return Ok(paperDtos);
        }
        [HttpPost("upload-and-spell-check")]
        public async Task<IActionResult> UploadAndSpellCheck(IFormFile pdfFile)
        {
            if (pdfFile == null || pdfFile.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                // Upload PDF lên Azure Blob
                string container = _configuration.GetValue<string>("BlobContainers:Papers");
                string fileUrl = await _azureBlobStorageService.UploadFileAsync(pdfFile, container);

                // Tải file PDF để đọc text + vị trí
                using var originalStream = await _azureBlobStorageService.DownloadFileAsync(fileUrl);
                using var pdfReader = new PdfReader(originalStream);
                using var pdfDoc = new PdfDocument(pdfReader);

                var allWordsWithPositions = new List<WordPosition>();
                string rawText = "";

                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    var strategy = new LocationTextExtractionStrategyWithPosition();
                    PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), strategy);

                    foreach (var wp in strategy.WordsPositions)
                    {
                        wp.PageNumber = i;
                        allWordsWithPositions.Add(wp);
                    }

                    rawText += strategy.GetResultantText() + "\n";
                }

                string cleanText = CleanExtractedText(rawText);

                // 1. Lấy danh sách từ sai
                List<string> misspelledWords = await _aiSpellCheckService.GetMisspelledWordsAsync(cleanText);

                // DEBUG: In danh sách từ sai chính tả từ AI
                Console.WriteLine("=== Misspelled Words from AI ===");
                foreach (var w in misspelledWords)
                    Console.WriteLine(w);

                // Hàm normalize: bỏ dấu câu, khoảng trắng thừa, lowercase
                string Normalize(string s) =>
                    Regex.Replace(s, @"[^\p{L}\p{N}]+", "") // chỉ giữ chữ cái & số
                          .Trim()
                          .ToLowerInvariant();

                // Chuẩn hóa danh sách từ sai chính tả
                var misspelledSet = new HashSet<string>(
                    misspelledWords
                        .Select(Normalize)
                        .Where(s => !string.IsNullOrEmpty(s))
                );

                // 2. Map từ sai -> vị trí
                var misspelledWordPositions = allWordsWithPositions
                    .Where(wp => misspelledSet.Contains(Normalize(wp.Word)))
                    .ToList();

                // DEBUG: In danh sách từ khớp trong PDF
                Console.WriteLine("=== Matched Misspelled Words in PDF ===");
                foreach (var wp in misspelledWordPositions)
                {
                    Console.WriteLine($"{wp.Word} (Page {wp.PageNumber}) - " +
                        $"X={wp.BoundingBox.GetX()}, Y={wp.BoundingBox.GetY()}, " +
                        $"W={wp.BoundingBox.GetWidth()}, H={wp.BoundingBox.GetHeight()}");
                }

                // 3. Highlight từ sai trong PDF
                var highlightedPdfPath = Path.Combine(Path.GetTempPath(), $"highlighted_{Guid.NewGuid()}.pdf");

                using (var reader = new PdfReader(await _azureBlobStorageService.DownloadFileAsync(fileUrl)))
                using (var writer = new PdfWriter(highlightedPdfPath))
                using (var pdfHighlightDoc = new PdfDocument(reader, writer))
                {
                    foreach (var wp in misspelledWordPositions)
                    {
                        var page = pdfHighlightDoc.GetPage(wp.PageNumber);
                        var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(page);

                        canvas.SaveState()
                              .SetFillColor(new iText.Kernel.Colors.DeviceRgb(255, 0, 0)) // Màu đỏ
                              .SetExtGState(new iText.Kernel.Pdf.Extgstate.PdfExtGState().SetFillOpacity(0.3f)) // Độ trong suốt 30%
                              .Rectangle(wp.BoundingBox)
                              .Fill()
                              .RestoreState();
                    }

                }

                // Upload file PDF highlight lên Azure Blob
                await using var highlightStream = System.IO.File.OpenRead(highlightedPdfPath);
                string highlightUrl = await _azureBlobStorageService
                    .UploadStreamAsync(highlightStream, Path.GetFileName(highlightedPdfPath), container, "application/pdf");

                return Ok(new
                {
                    FileUrl = fileUrl,
                    HighlightedFileUrl = highlightUrl,
                    OriginalTextPreview = cleanText.Substring(0, Math.Min(500, cleanText.Length)),
                    MisspelledWords = misspelledWords,
                    MisspelledWordPositions = misspelledWordPositions.Select(wp => new
                    {
                        wp.Word,
                        wp.PageNumber,
                        BoundingBox = new
                        {
                            X = wp.BoundingBox.GetX(),
                            Y = wp.BoundingBox.GetY(),
                            Width = wp.BoundingBox.GetWidth(),
                            Height = wp.BoundingBox.GetHeight()
                        }
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }



        private string CleanExtractedText(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            // 1. Thay thế xuống dòng, tab, carriage return thành khoảng trắng
            string cleaned = Regex.Replace(text, @"[\r\n\t]+", " ");

            // 2. Loại bỏ các ký tự không in được hoặc lạ (bạn có thể tùy chỉnh thêm)
            cleaned = Regex.Replace(cleaned, @"[^\u0009\u000A\u000D\u0020-\u007E]", "");

            // 3. Chuẩn hóa khoảng trắng nhiều thành 1 khoảng trắng
            cleaned = Regex.Replace(cleaned, @"\s{2,}", " ");

            // 4. Trim đầu cuối
            return cleaned.Trim();
        }

        private async Task<string> RunSpellCheckInChunks(string text, int chunkSize = 2000)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var parts = new List<string>();
            for (int i = 0; i < text.Length; i += chunkSize)
            {
                var chunk = text.Substring(i, Math.Min(chunkSize, text.Length - i));
                var res = await _aiSpellCheckService.CheckSpellingAsync(chunk);
                parts.Add(res);
            }
            return string.Join("\n", parts);
        }

        // ... Trong PaperController
        [HttpGet("translate-pdf/{paperId}")]
        public async Task<IActionResult> TranslatePaperPdf(int paperId, string targetLang)
        {
            var paper = await _paperRepository.GetPaperByIdAsync(paperId);
            if (paper == null)
            {
                return NotFound("Paper not found.");
            }

            try
            {
                // Gọi một hàm duy nhất từ PdfService để vừa tải vừa trích xuất
                var paperText = await _pdfService.ExtractTextFromPdfAsync(paper.FilePath);

                var translatedText = await _translationService.TranslateAsync(paperText, targetLang);

                return Ok(new { OriginalText = paperText, TranslatedText = translatedText });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        
        }


    }
}
