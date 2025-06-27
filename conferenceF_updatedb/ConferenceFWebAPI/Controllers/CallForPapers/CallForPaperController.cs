using ConferenceFWebAPI.DTOs.CallForPapers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository;

namespace ConferenceFWebAPI.Controllers.CallForPaper
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallForPaperController : ControllerBase
    {
        private readonly ICallForPaperRepository _callForPaperRepository;
        private readonly IWebHostEnvironment _webHostEnvironment; // Thêm để truy cập đường dẫn vật lý

        public CallForPaperController(ICallForPaperRepository callForPaperRepository, IWebHostEnvironment webHostEnvironment)
        {
            _callForPaperRepository = callForPaperRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: api/CallForPaper
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CallForPaperDto>))] // Cập nhật kiểu trả về
        public async Task<ActionResult<IEnumerable<CallForPaperDto>>> GetCallForPapers()
        {
            var callForPapers = await _callForPaperRepository.GetAllCallForPapers();
            // Map Entity sang DTO để trả về
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CallForPaperDto))] // Cập nhật kiểu trả về
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CallForPaperDto>> GetCallForPaper(int id)
        {
            var callForPaper = await _callForPaperRepository.GetCallForPaperById(id);

            if (callForPaper == null)
            {
                return NotFound();
            }

            // Map Entity sang DTO để trả về
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CallForPaperDto>))] // Cập nhật kiểu trả về
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<CallForPaperDto>>> GetCallForPapersByConferenceId(int conferenceId)
        {
            var callForPapers = await _callForPaperRepository.GetCallForPapersByConferenceId(conferenceId);

            if (callForPapers == null || !callForPapers.Any())
            {
                return NotFound($"Không tìm thấy CallForPaper nào cho ConferenceId: {conferenceId}");
            }

            // Map Entity sang DTO để trả về
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
        // Sẽ nhận dữ liệu từ form (multipart/form-data)
        [HttpPost]
        [Consumes("multipart/form-data")] // Chỉ định kiểu dữ liệu đầu vào
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CallForPaperDto))] // Cập nhật kiểu trả về
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CallForPaperDto>> PostCallForPaper([FromForm] CallForPaperCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string? templatePath = null;
            if (createDto.TemplateFile != null)
            {
                // Xử lý lưu file
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "callforpapers");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + createDto.TemplateFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await createDto.TemplateFile.CopyToAsync(fileStream);
                }
                templatePath = Path.Combine("uploads", "callforpapers", uniqueFileName); // Lưu đường dẫn tương đối
            }

            var callForPaper = new BussinessObject.Entity.CallForPaper
            {
                ConferenceId = createDto.ConferenceId,
                Description = createDto.Description,
                Deadline = createDto.Deadline,
                TemplatePath = templatePath, // Đường dẫn của file đã lưu
                Status = true, // Mặc định là TRUE khi tạo mới
                CreatedAt = DateTime.UtcNow // Đặt thời gian tạo
            };

            await _callForPaperRepository.AddCallForPaper(callForPaper);

            // Map Entity sang DTO để trả về
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

        // PUT: api/CallForPaper/5
        // Sẽ nhận dữ liệu từ form (multipart/form-data)
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")] // Chỉ định kiểu dữ liệu đầu vào
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutCallForPaper(int id, [FromForm] CallForPaperUpdateDto updateDto)
        {
            if (id != updateDto.Cfpid)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingCallForPaper = await _callForPaperRepository.GetCallForPaperById(id);
            if (existingCallForPaper == null)
            {
                return NotFound();
            }

            // Cập nhật các thuộc tính từ DTO
            existingCallForPaper.ConferenceId = updateDto.ConferenceId;
            existingCallForPaper.Description = updateDto.Description;
            existingCallForPaper.Deadline = updateDto.Deadline;

            // Xử lý file mới nếu có
            if (updateDto.TemplateFile != null)
            {
                // Xóa file cũ nếu có
                if (!string.IsNullOrEmpty(existingCallForPaper.TemplatePath))
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingCallForPaper.TemplatePath);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Lưu file mới
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "callforpapers");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + updateDto.TemplateFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await updateDto.TemplateFile.CopyToAsync(fileStream);
                }
                existingCallForPaper.TemplatePath = Path.Combine("uploads", "callforpapers", uniqueFileName); // Cập nhật đường dẫn
            }

            try
            {
                await _callForPaperRepository.UpdateCallForPaper(existingCallForPaper);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Bạn có thể thêm logic kiểm tra riêng ở đây nếu cần
                // Ví dụ: if (!CallForPaperExists(id)) { return NotFound(); } else { throw; }
                throw;
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

            // Xóa file vật lý nếu có
            if (!string.IsNullOrEmpty(callForPaper.TemplatePath))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, callForPaper.TemplatePath);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            await _callForPaperRepository.DeleteCallForPaper(id);
            return NoContent();
        }
    }
}
