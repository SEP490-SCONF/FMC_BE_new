using ConferenceFWebAPI.DTOs.CallForPapers;
using ConferenceFWebAPI.Service; // Thêm namespace của Azure Blob Storage Service
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository;
using BussinessObject.Entity; // Thêm namespace cho Entity
using Microsoft.Extensions.Configuration; // Thêm để đọc config

namespace ConferenceFWebAPI.Controllers.CallForPaper
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallForPaperController : ControllerBase
    {
        private readonly ICallForPaperRepository _callForPaperRepository;
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly IConfiguration _configuration;

        public CallForPaperController(
            ICallForPaperRepository callForPaperRepository,
            IAzureBlobStorageService azureBlobStorageService,
            IConfiguration configuration)
        {
            _callForPaperRepository = callForPaperRepository;
            _azureBlobStorageService = azureBlobStorageService;
            _configuration = configuration;
        }

        // GET: api/CallForPaper
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CallForPaperDto>))]
        public async Task<ActionResult<IEnumerable<CallForPaperDto>>> GetCallForPapers()
        {
            var callForPapers = await _callForPaperRepository.GetAllCallForPapers();
            var callForPaperDtos = callForPapers.Select(cf => new CallForPaperDto
            {
                Cfpid = cf.Cfpid,
                ConferenceId = cf.ConferenceId,
                Description = cf.Description,
                Deadline = cf.Deadline,
                TemplatePath = cf.TemplatePath,
                CreatedAt = cf.CreatedAt
            }).ToList();
            return Ok(callForPaperDtos);
        }

        // GET: api/CallForPaper/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CallForPaperDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CallForPaperDto>> GetCallForPaper(int id)
        {
            var callForPaper = await _callForPaperRepository.GetCallForPaperById(id);
            if (callForPaper == null)
            {
                return NotFound();
            }
            var callForPaperDto = new CallForPaperDto
            {
                Cfpid = callForPaper.Cfpid,
                ConferenceId = callForPaper.ConferenceId,
                Description = callForPaper.Description,
                Deadline = callForPaper.Deadline,
                TemplatePath = callForPaper.TemplatePath,
                CreatedAt = callForPaper.CreatedAt
            };
            return Ok(callForPaperDto);
        }

        // GET: api/CallForPaper/byconference/1
        [HttpGet("byconference/{conferenceId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CallForPaperDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<CallForPaperDto>>> GetCallForPapersByConferenceId(int conferenceId)
        {
            var callForPapers = await _callForPaperRepository.GetCallForPapersByConferenceId(conferenceId);

            if (callForPapers == null || !callForPapers.Any())
            {
                return NotFound($"Không tìm thấy CallForPaper nào cho ConferenceId: {conferenceId}");
            }
            
            var callForPaperDtos = callForPapers.Select(cf => new CallForPaperDto
            {
                Cfpid = cf.Cfpid,
                ConferenceId = cf.ConferenceId,
                Description = cf.Description,
                Deadline = cf.Deadline,
                TemplatePath = cf.TemplatePath,
                CreatedAt = cf.CreatedAt
            }).ToList();

            return Ok(callForPaperDtos);
        }


        // POST: api/CallForPaper
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CallForPaperDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CallForPaperDto>> PostCallForPaper([FromForm] CallForPaperCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                string? templateUrl = null;
                // ** [THAY ĐỔI] UPLOAD FILE LÊN AZURE STORAGE **
                if (createDto.TemplateFile != null && createDto.TemplateFile.Length > 0)
                {
                    // Lấy tên container từ appsettings.json
                    var containerName = _configuration.GetValue<string>("BlobContainers:CallForPapers");
                    if (string.IsNullOrEmpty(containerName))
                    {
                        return StatusCode(500, "CallForPapers storage container name is not configured.");
                    }

                    // Gọi service để upload và nhận lại URL
                    templateUrl = await _azureBlobStorageService.UploadFileAsync(createDto.TemplateFile, containerName);
                }

                var callForPaper = new BussinessObject.Entity.CallForPaper
                {
                    ConferenceId = createDto.ConferenceId,
                    Description = createDto.Description,
                    Deadline = createDto.Deadline,
                    TemplatePath = templateUrl, // Lưu URL từ Azure
                    Status = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _callForPaperRepository.AddCallForPaper(callForPaper);

                var callForPaperDto = new CallForPaperDto
                {
                    Cfpid = callForPaper.Cfpid,
                    ConferenceId = callForPaper.ConferenceId,
                    Description = callForPaper.Description,
                    Deadline = callForPaper.Deadline,
                    TemplatePath = callForPaper.TemplatePath,
                    CreatedAt = callForPaper.CreatedAt
                };

                return CreatedAtAction(nameof(GetCallForPaper), new { id = callForPaper.Cfpid }, callForPaperDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/CallForPaper/5
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutCallForPaper(int id, [FromForm] CallForPaperUpdateDto updateDto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingCallForPaper = await _callForPaperRepository.GetCallForPaperById(id);
            if (existingCallForPaper == null)
            {
                return NotFound();
            }

            try
            {
                // ** [THAY ĐỔI] XỬ LÝ CẬP NHẬT FILE TRÊN AZURE STORAGE **
                if (updateDto.TemplateFile != null && updateDto.TemplateFile.Length > 0)
                {
                    // 1. Xóa file cũ trên Azure nếu có
                    if (!string.IsNullOrEmpty(existingCallForPaper.TemplatePath))
                    {
                        await _azureBlobStorageService.DeleteFileAsync(existingCallForPaper.TemplatePath);
                    }

                    // 2. Tải file mới lên và cập nhật đường dẫn
                    var containerName = _configuration.GetValue<string>("BlobContainers:CallForPapers");
                    var newTemplateUrl = await _azureBlobStorageService.UploadFileAsync(updateDto.TemplateFile, containerName);
                    existingCallForPaper.TemplatePath = newTemplateUrl;
                }

                // Cập nhật các thuộc tính khác
                existingCallForPaper.ConferenceId = updateDto.ConferenceId;
                existingCallForPaper.Description = updateDto.Description;
                existingCallForPaper.Deadline = updateDto.Deadline;
                
                await _callForPaperRepository.UpdateCallForPaper(existingCallForPaper);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw; // Re-throw để middleware xử lý hoặc trả về lỗi
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error while updating: {ex.Message}");
            }

            return NoContent();
        }

        // DELETE: api/CallForPaper/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCallForPaper(int id)
        {
            var callForPaper = await _callForPaperRepository.GetCallForPaperById(id);
            if (callForPaper == null)
            {
                return NotFound();
            }
            
            try
            {
                // ** [THAY ĐỔI] XÓA FILE TRÊN AZURE STORAGE **
                if (!string.IsNullOrEmpty(callForPaper.TemplatePath))
                {
                    await _azureBlobStorageService.DeleteFileAsync(callForPaper.TemplatePath);
                }

                // Xóa record trong database
                await _callForPaperRepository.DeleteCallForPaper(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error while deleting: {ex.Message}");
            }

            return NoContent();
        }
    }
}
