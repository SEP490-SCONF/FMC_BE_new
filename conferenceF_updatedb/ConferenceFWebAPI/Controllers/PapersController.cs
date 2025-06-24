using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.Paper;
using ConferenceFWebAPI.Service;
using DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Repository;

namespace ConferenceFWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PapersController : ControllerBase
    {
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly IPaperRepository _paperRepository;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        public PapersController(IAzureBlobStorageService azureBlobStorageService,
                                IPaperRepository paperRepository,
                                IConfiguration configuration,
                                IMapper mapper)
        {
            _azureBlobStorageService = azureBlobStorageService;
            _paperRepository = paperRepository;
            _configuration = configuration;
            _mapper = mapper;
        }


        [HttpPost("upload-pdf")]
        public async Task<IActionResult> UploadPdf([FromForm] PaperUploadDto paperDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (paperDto.PdfFile == null || paperDto.PdfFile.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            if (Path.GetExtension(paperDto.PdfFile.FileName)?.ToLower() != ".pdf")
            {
                return BadRequest("Only PDF files are allowed.");
            }

            // Thêm kiểm tra AuthorIds không rỗng
            if (paperDto.AuthorIds == null || !paperDto.AuthorIds.Any())
            {
                return BadRequest("At least one author must be provided.");
            }

            try
            {
                var paperContainerName = _configuration.GetValue<string>("BlobContainers:Papers");
                if (string.IsNullOrEmpty(paperContainerName))
                {
                    return StatusCode(500, "Blob storage container name is not configured.");
                }

                string fileUrl = await _azureBlobStorageService.UploadFileAsync(paperDto.PdfFile, paperContainerName);

                var paper = _mapper.Map<Paper>(paperDto);

                paper.FilePath = fileUrl;
                paper.SubmitDate = DateTime.UtcNow;
                paper.Status = "Submitted";
                paper.IsPublished = false;

                paper.PaperAuthors = new List<PaperAuthor>();
                int authorOrder = 1; // Khởi tạo thứ tự tác giả
                foreach (var authorId in paperDto.AuthorIds.Distinct()) 
                {
                    paper.PaperAuthors.Add(new PaperAuthor
                    {
                        AuthorId = authorId,
                        AuthorOrder = authorOrder 
                    });
                    authorOrder++;
                }

                await _paperRepository.AddPaperAsync(paper);

                return Ok(new { Message = "File uploaded and paper data saved successfully.", FileUrl = fileUrl, PaperId = paper.PaperId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi chi tiết ở đây
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [EnableQuery] // Vẫn crucial cho OData query options
        public IActionResult Get()
        {
            var papersQuery = _paperRepository.GetAllPapers();

            if (papersQuery == null)
            {
                return NotFound("No papers found.");
            }

            var paperDtos = _mapper.ProjectTo<PaperResponseDto>(papersQuery);

            if (!paperDtos.Any())
            {
                return NotFound("No active papers found.");
            }

            return Ok(paperDtos); 
        }

        [HttpGet("{key}")]
        [EnableQuery] 
        public async Task<IActionResult> Get([FromRoute] int key)
        {
            var paper = await _paperRepository.GetPaperByIdAsync(key);
            if (paper == null)
            {
                return NotFound($"Paper with ID {key} not found.");
            }
 
            var paperDto = _mapper.Map<PaperResponseDto>(paper);
            return Ok(paperDto);
        }

        [HttpPut("mark-as-deleted/{paperId}")]
        public async Task<IActionResult> MarkPaperAsDeleted(int paperId) 
        {
            var paper = await _paperRepository.GetPaperByIdAsync(paperId);
            if (paper == null)
            {
                return NotFound("Paper not found.");
            }

            try
            {
                paper.Status = "Deleted";

                await _paperRepository.UpdatePaperAsync(paper);


                return Ok($"Paper with ID {paperId} successfully marked as 'Deleted'.");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi chi tiết ở đây
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("{paperId}/publish")] // <-- Route mới cho việc xuất bản/bỏ xuất bản
        public async Task<IActionResult> UpdatePaperPublishStatus(int paperId, [FromBody] PaperPublishDto publishDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var paper = await _paperRepository.GetPaperByIdAsync(paperId);
            if (paper == null)
            {
                return NotFound($"Paper with ID {paperId} not found.");
            }

            try
            {
                // Cập nhật trạng thái IsPublished của bài báo
                paper.IsPublished = publishDto.IsPublished;

                // Gọi Repository để cập nhật vào database
                await _paperRepository.UpdatePaperAsync(paper);

                return Ok($"Paper with ID {paperId} IsPublished status updated to {publishDto.IsPublished}.");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi chi tiết ở đây nếu cần
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
