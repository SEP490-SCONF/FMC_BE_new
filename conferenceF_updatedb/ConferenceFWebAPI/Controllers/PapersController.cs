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
        private readonly IPaperRevisionRepository _paperRevisionRepository;

        public PapersController(IAzureBlobStorageService azureBlobStorageService,
                                IPaperRepository paperRepository,
                                IConfiguration configuration,
                                IMapper mapper,
                                IPaperRevisionRepository paperRevisionRepository)
        {
            _azureBlobStorageService = azureBlobStorageService;
            _paperRepository = paperRepository;
            _configuration = configuration;
            _mapper = mapper;
            _paperRevisionRepository = paperRevisionRepository;

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



                //await _paperRepository.AddPaperAsync(paper);

                //return Ok(new { Message = "File uploaded and paper data saved successfully.", FileUrl = fileUrl, PaperId = paper.PaperId });
                await _paperRepository.AddPaperAsync(paper);

                // Sau khi thêm Paper, tạo Revision đầu tiên
                var initialRevision = new PaperRevision
                {
                    PaperId = paper.PaperId,
                    FilePath = fileUrl,
                    Status = "Initial",
                    SubmittedAt = DateTime.Now
                };
                await _paperRevisionRepository.AddPaperRevisionAsync(initialRevision);

                return Ok(new
                {
                    Message = "File uploaded and paper + initial revision saved successfully.",
                    FileUrl = fileUrl,
                    PaperId = paper.PaperId,
                    RevisionId = initialRevision.RevisionId
                });

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
    }
}
