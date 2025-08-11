using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.PaperRevisions;
using ConferenceFWebAPI.DTOs.UserConferenceRoles;
using ConferenceFWebAPI.Hubs;
using ConferenceFWebAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Repository;
using Repository.Repository;

namespace ConferenceFWebAPI.Controllers.PaperRevisions
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaperRevisionsController : ControllerBase
    {
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly IPaperRevisionRepository _paperRevisionRepository;
        private readonly IPaperRepository _paperRepository; // Cần để kiểm tra PaperId tồn tại
        private readonly IReviewRepository _reviewRepository;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IReviewerAssignmentRepository _reviewerAssignmentRepository;

        public PaperRevisionsController(IAzureBlobStorageService azureBlobStorageService,
                                        IPaperRevisionRepository paperRevisionRepository,
                                        IPaperRepository paperRepository, // Inject PaperRepository
                                        IConfiguration configuration,
                                        IMapper mapper,
                                        IReviewRepository reviewRepository,
                                        IUserRepository userRepository,
                                        INotificationRepository notificationRepository,
                                        IHubContext<NotificationHub> hubContext,
                                        IReviewerAssignmentRepository reviewerAssignmentRepository)
        {
            _azureBlobStorageService = azureBlobStorageService;
            _paperRevisionRepository = paperRevisionRepository;
            _paperRepository = paperRepository;
            _configuration = configuration;
            _mapper = mapper;
            _reviewRepository = reviewRepository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
            _reviewerAssignmentRepository = reviewerAssignmentRepository;
        }


        // POST: api/PaperRevisions/upload-revision
        [HttpPost("upload-revision")]
        public async Task<IActionResult> UploadRevision([FromForm] PaperRevisionUploadDto revisionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (revisionDto.PdfFile == null || revisionDto.PdfFile.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            if (Path.GetExtension(revisionDto.PdfFile.FileName)?.ToLower() != ".pdf")
            {
                return BadRequest("Only PDF files are allowed for revisions.");
            }

            var existingPaper = await _paperRepository.GetPaperByIdAsync(revisionDto.PaperId);
            if (existingPaper == null)
            {
                return NotFound($"Paper with ID {revisionDto.PaperId} not found.");
            }

            try
            {
                var revisionContainerName = _configuration.GetValue<string>("BlobContainers:PaperRevisions");
                if (string.IsNullOrEmpty(revisionContainerName))
                {
                    return StatusCode(500, "Blob storage container name for paper revisions is not configured.");
                }

                string fileUrl = await _azureBlobStorageService.UploadFileAsync(revisionDto.PdfFile, revisionContainerName);

                var paperRevision = _mapper.Map<PaperRevision>(revisionDto);
                paperRevision.FilePath = fileUrl;
                paperRevision.Status = "Under Review";
                paperRevision.SubmittedAt = DateTime.UtcNow;

                await _paperRevisionRepository.AddPaperRevisionAsync(paperRevision);

                existingPaper.Status = "Under Review";
                await _paperRepository.UpdatePaperAsync(existingPaper);

                // Send notifications to authors
                var paperAuthors = await _paperRepository.GetAuthorsByPaperIdAsync(existingPaper.PaperId);
                foreach (var pa in paperAuthors)
                {
                    var authorUser = await _userRepository.GetById(pa.AuthorId);
                    if (authorUser != null)
                    {
                        string notificationTitle = "Paper Resubmission Successful!";
                        string notificationContent = $"Your revised paper '{existingPaper.Title}' has been successfully submitted.";

                        var notification = new Notification
                        {
                            Title = notificationTitle,
                            Content = notificationContent,
                            UserId = authorUser.UserId,
                            RoleTarget = "Author",
                            CreatedAt = DateTime.UtcNow
                        };
                        await _notificationRepository.AddNotificationAsync(notification);
                        await _hubContext.Clients.User(authorUser.UserId.ToString()).SendAsync("ReceiveNotification", notificationTitle, notificationContent);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Author user with ID {pa.AuthorId} not found for notification.");
                    }
                }

                // Send notifications to reviewers
                var reviewerAssignments = await _reviewerAssignmentRepository.GetReviewersByPaperIdAsync(existingPaper.PaperId);
                foreach (var ra in reviewerAssignments)
                {
                    var reviewerUser = await _userRepository.GetById(ra.ReviewerId);
                    if (reviewerUser != null)
                    {
                        string notificationTitle = "Paper Updated!";
                        string notificationContent = $"The paper '{existingPaper.Title}' has been updated with a new revision by the author. Please check for changes.";

                        var notification = new Notification
                        {
                            Title = notificationTitle,
                            Content = notificationContent,
                            UserId = ra.ReviewerId,
                            RoleTarget = "Reviewer",
                            CreatedAt = DateTime.UtcNow
                        };
                        await _notificationRepository.AddNotificationAsync(notification);
                        await _hubContext.Clients.User(reviewerUser.UserId.ToString()).SendAsync("ReceiveNotification", notificationTitle, notificationContent);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Reviewer user with ID {ra.ReviewerId} not found for notification.");
                    }
                }

                return Ok(new
                {
                    Message = "Paper revision uploaded and data saved successfully. Notifications sent to authors and reviewers.",
                    FileUrl = fileUrl,
                    RevisionId = paperRevision.RevisionId
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Upload error: {ex.ToString()}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        // GET: api/PaperRevisions/{revisionId}
        [HttpGet("{revisionId}")]
        public async Task<IActionResult> GetRevisionById(int revisionId)
        {
            var revision = await _paperRevisionRepository.GetPaperRevisionByIdAsync(revisionId);
            if (revision == null)
            {
                return NotFound("Paper revision not found.");
            }
            var revisionDto = _mapper.Map<PaperRevisionResponseDto>(revision);
            return Ok(revisionDto);
        }

        // GET: api/PaperRevisions/paper/{paperId}
        [HttpGet("paper/{paperId}")]
        public async Task<IActionResult> GetRevisionsByPaperId(int paperId)
        {
            var revisions = await _paperRevisionRepository.GetRevisionsByPaperIdAsync(paperId);
            if (revisions == null || !((List<PaperRevision>)revisions).Any())
            {
                return NotFound($"No revisions found for Paper ID {paperId}.");
            }
            var revisionDtos = _mapper.Map<IEnumerable<PaperRevisionResponseDto>>(revisions);
            return Ok(revisionDtos);
        }


        // GET: api/PaperRevisions/view-pdf/{revisionId}
        [HttpGet("view-pdf/{revisionId}")]
        public async Task<IActionResult> ViewPdf(int revisionId)
        {
            var revision = await _paperRevisionRepository.GetPaperRevisionByIdAsync(revisionId);
            if (revision == null || string.IsNullOrEmpty(revision.FilePath))
            {
                return NotFound("Paper revision or PDF file not found.");
            }

            // Chuyển hướng đến URL của file trên Azure Blob Storage
            return Redirect(revision.FilePath);
        }
        
        // DELETE: api/PaperRevisions/delete-revision/{revisionId}
        [HttpDelete("delete-revision/{revisionId}")]
        public async Task<IActionResult> DeleteRevision(int revisionId)
        {
            var revisionToDelete = await _paperRevisionRepository.GetPaperRevisionByIdAsync(revisionId);
            if (revisionToDelete == null)
            {
                return NotFound($"Paper revision with ID {revisionId} not found.");
            }

            try
            {
                // Xóa file khỏi Azure Blob Storage nếu FilePath tồn tại
                if (!string.IsNullOrEmpty(revisionToDelete.FilePath))
                {
                    bool isDeletedFromBlob = await _azureBlobStorageService.DeleteFileAsync(revisionToDelete.FilePath);
                    if (!isDeletedFromBlob)
                    {
                        // Ghi log nếu xóa blob không thành công nhưng vẫn cố gắng xóa bản ghi DB
                        // return StatusCode(500, "Failed to delete PDF file from Azure Blob Storage.");
                        // Hoặc bạn có thể chọn tiếp tục xóa bản ghi DB ngay cả khi blob không xóa được
                    }
                }

                // Xóa bản ghi khỏi database
                await _paperRevisionRepository.DeletePaperRevisionAsync(revisionId);

                return Ok($"Paper revision with ID {revisionId} and associated PDF deleted successfully.");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi chi tiết ở đây
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
     
        }
        // GET: api/Review/PaperRevisionUrl/{reviewId}
        [HttpGet("PaperRevisionUrl/{reviewId}")]
        public async Task<IActionResult> GetPdfUrlByReviewId(int reviewId)
        {
            // Lấy Review từ reviewId
            var review = await _reviewRepository.GetById(reviewId);
            if (review == null)
            {
                return NotFound($"Review with ID {reviewId} not found.");
            }

            // Lấy PaperRevision từ RevisionId của review
            var paperRevision = await _paperRevisionRepository.GetPaperRevisionByIdAsync(review.RevisionId);
            if (paperRevision == null)
            {
                return NotFound($"PaperRevision with RevisionId {review.RevisionId} not found.");
            }

            // Lấy URL PDF từ PaperRevision
            var pdfUrl = paperRevision.FilePath;  // Giả sử bạn lưu trữ đường dẫn file PDF trong trường PdfUrl

            if (string.IsNullOrEmpty(pdfUrl))
            {
                return NotFound("PDF URL is not available for this PaperRevision.");
            }

            // Trả về PDF URL
            return Ok(new { PdfUrl = pdfUrl });
        }

    }
}
