using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs;
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



                await _paperRepository.AddPaperAsync(paper);

                return Ok(new { Message = "File uploaded and paper data saved successfully.", FileUrl = fileUrl, PaperId = paper.PaperId });
            }
            catch (ArgumentException ex) 
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
             
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [EnableQuery] // <-- Crucial for OData query options
        public IActionResult Get() // Changed name from GetAllPapers to Get to follow OData convention
        {

            var papers = _paperRepository.GetAllPapers(); // Giả định có phương thức này hoặc tương tự

            if (papers == null)
            {
                return NotFound("No papers found.");
            }
            return Ok(papers); // OData sẽ tự động xử lý các truy vấn trên IQueryable này
        }

        [HttpGet("view-pdf/{paperId}")]
        [EnableQuery]
        public async Task<IActionResult> ViewPdf(int paperId)
        {
            var paper = await _paperRepository.GetPaperByIdAsync(paperId);
            if (paper == null || string.IsNullOrEmpty(paper.FilePath))
            {
                return NotFound("Paper or PDF file not found.");
            }

            return Redirect(paper.FilePath);
        }


        [HttpPut("mark-as-deleted/{paperId}")] 
        public async Task<IActionResult> MarkPaperAsDeleted(int paperId) // <-- Đổi tên hàm
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
    }
}
