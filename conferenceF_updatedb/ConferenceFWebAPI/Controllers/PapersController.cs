using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.Paper;
using ConferenceFWebAPI.DTOs.Papers;
using ConferenceFWebAPI.Hubs;
using ConferenceFWebAPI.Service;
using DataAccess;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.SignalR;
using Repository;
using System.Security.Claims;
using System.Text;

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
            PaperDeadlineService paperDeadlineService, // THÊM VÀO CONSTRUCTOR
            ITimeLineRepository timeLineRepository,
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

                // THÊM LƯU THÔNG BÁO CHO NGƯỜI NỘP BÀI THÀNH CÔNG
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
                }

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

                // Chỉ trả về translatedText thay vì cả OriginalText
                return Ok(new { TranslatedText = translatedText });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
