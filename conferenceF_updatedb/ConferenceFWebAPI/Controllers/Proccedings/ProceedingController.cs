using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.Proccedings;
using ConferenceFWebAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace ConferenceFWebAPI.Controllers.Proccedings
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProceedingsController : ControllerBase
    {
        private readonly IProceedingRepository _repo;
        private readonly IAzureBlobStorageService _blobService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IPaperRevisionRepository _paperRevisionRepo;


        public ProceedingsController(IProceedingRepository repo, IAzureBlobStorageService blobService, IMapper mapper, IConfiguration config, IPaperRevisionRepository revisionRepo)
        {
            _repo = repo;
            _blobService = blobService;
            _mapper = mapper;
            _config = config;
            _paperRevisionRepo = revisionRepo;
        }

        //[HttpPost]
        //public async Task<IActionResult> Create([FromForm] ProceedingCreateDto dto)
        //{
        //    if (dto.File == null || dto.File.Length == 0)
        //        return BadRequest("PDF file is required.");

        //    var container = _config["BlobContainers:Proceedings"];
        //    var fileUrl = await _blobService.UploadFileAsync(dto.File, container);

        //    var proceeding = new Proceeding
        //    {
        //        ConferenceId = dto.ConferenceId,
        //        Title = dto.Title,
        //        Description = dto.Description,
        //        FilePath = fileUrl,
        //        PublishedDate = DateTime.UtcNow,
        //        CreatedAt = DateTime.UtcNow,
        //        UpdatedAt = DateTime.UtcNow,
        //        PublishedBy = 1
        //    };

        //    await _repo.Add(proceeding);
        //    return Ok(new { message = "Proceeding created", proceedingId = proceeding.ProceedingId });
        //}

        [HttpPost("from-paper")]
        public async Task<IActionResult> CreateFromPaper([FromBody] ProceedingCreateFromPaperDto dto)
        {
            var filePath = await _paperRevisionRepo.GetAcceptedFilePathByPaperIdAsync(dto.PaperId);
            if (filePath == null)
                return NotFound("Không tìm thấy revision nào đã được accepted cho Paper này.");

            var proceeding = new Proceeding
            {
                ConferenceId = dto.ConferenceId,
                Title = dto.Title,
                Description = dto.Description,
                FilePath = filePath,
                PublishedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PublishedBy = 1 
            };

            await _repo.Add(proceeding);
            return Ok(new { message = "Proceeding created from PaperRevision", proceedingId = proceeding.ProceedingId });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ProceedingUpdateDto dto)
        {
            var proceeding = await _repo.GetById(dto.ProceedingId);
            if (proceeding == null)
                return NotFound();

            proceeding.Title = dto.Title;
            proceeding.Description = dto.Description;
            proceeding.UpdatedAt = DateTime.UtcNow;

            await _repo.Update(proceeding);
            return Ok("Proceeding updated.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var proceeding = await _repo.GetById(id);
            if (proceeding == null) return NotFound();

            await _repo.Delete(id);
            return Ok("Proceeding deleted.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var proceeding = await _repo.GetById(id);
            if (proceeding == null) return NotFound();

            var dto = _mapper.Map<ProceedingResponseDto>(proceeding);
            return Ok(dto);
        }

        [HttpGet("conference/{conferenceId}")]
        public async Task<IActionResult> GetByConference(int conferenceId)
        {
            var proceedings = await _repo.GetByConferenceId(conferenceId);
            var result = proceedings.Select(p => new ProceedingResponseDto
            {
                ProceedingId = p.ProceedingId,
                Title = p.Title,
                Description = p.Description,
                FilePath = p.FilePath,
                PublishedDate = p.PublishedDate,
                PublishedByName = p.PublishedByNavigation?.Name
            }).ToList();

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var proceedings = await _repo.GetAll();
            var result = proceedings.Select(p => _mapper.Map<ProceedingResponseDto>(p));
            return Ok(result);
        }


    }
}
