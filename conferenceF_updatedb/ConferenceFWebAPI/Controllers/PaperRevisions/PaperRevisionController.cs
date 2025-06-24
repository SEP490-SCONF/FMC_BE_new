using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.PaperRevisions;
using ConferenceFWebAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace ConferenceFWebAPI.Controllers.PaperRevisions
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaperRevisionsController : ControllerBase
    {
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly IPaperRevisionRepository _paperRevisionRepository;
        private readonly IPaperRepository _paperRepository; // Cần để kiểm tra PaperId tồn tại
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public PaperRevisionsController(IAzureBlobStorageService azureBlobStorageService,
                                        IPaperRevisionRepository paperRevisionRepository,
                                        IPaperRepository paperRepository, // Inject PaperRepository
                                        IConfiguration configuration,
                                        IMapper mapper)
        {
            _azureBlobStorageService = azureBlobStorageService;
            _paperRevisionRepository = paperRevisionRepository;
            _paperRepository = paperRepository;
            _configuration = configuration;
            _mapper = mapper;
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

            // Kiểm tra xem PaperId có tồn tại không
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

                // Upload file lên Azure Blob Storage
                string fileUrl = await _azureBlobStorageService.UploadFileAsync(revisionDto.PdfFile, revisionContainerName);

                // Map DTO sang Entity và thiết lập các trường
                var paperRevision = _mapper.Map<PaperRevision>(revisionDto);
                paperRevision.FilePath = fileUrl;
                paperRevision.Status = "PendingReview"; // Hoặc trạng thái mặc định khác, ví dụ: "PendingReview"
                paperRevision.SubmittedAt = DateTime.UtcNow;

                // Lưu thông tin bản sửa đổi vào cơ sở dữ liệu
                await _paperRevisionRepository.AddPaperRevisionAsync(paperRevision);

                return Ok(new
                {
                    Message = "Paper revision uploaded and data saved successfully.",
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
                // Nên ghi log lỗi chi tiết ở đây
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
    }
}
