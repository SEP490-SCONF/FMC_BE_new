using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.Proccedings;
using ConferenceFWebAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace ConferenceFWebAPI.Controllers.Proccedings
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProceedingController : ControllerBase
    {
        private readonly IProceedingRepository _proceedingRepository;
        private readonly IPaperRepository _paperRepository;
        private readonly IUserRepository _userRepository;

        public ProceedingController(IProceedingRepository proceedingRepository, IPaperRepository paperRepository, IUserRepository userRepository)
        {
            _proceedingRepository = proceedingRepository;
            _paperRepository = paperRepository;
            _userRepository = userRepository;
        }

        // POST: api/Proceeding/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateProceeding([FromBody] ProceedingCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingProceeding = await _proceedingRepository.GetProceedingByConferenceIdAsync(dto.ConferenceId);
            if (existingProceeding != null)
            {
                return BadRequest("A proceeding for this conference already exists.");
            }

            var newProceeding = new Proceeding
            {
                ConferenceId = dto.ConferenceId,
                Title = dto.Title,
                Description = dto.Description,
                PublishedBy = dto.PublishedBy,
                PublishedDate = DateTime.UtcNow,
                Status = "Published",
                FilePath = dto.FilePath,
                Doi = dto.Doi,
                Version = "1.0"
            };

            try
            {
                var createdProceeding = await _proceedingRepository.CreateProceedingAsync(newProceeding);

                if (dto.PaperIds != null && dto.PaperIds.Any())
                {
                    foreach (var paperId in dto.PaperIds)
                    {
                        var paper = await _paperRepository.GetPaperByIdAsync(paperId);
                        if (paper != null)
                        {
                            paper.IsPublished = true;
                            await _paperRepository.UpdatePaperAsync(paper);
                        }
                    }
                }

                return CreatedAtAction(nameof(GetProceeding), new { proceedingId = createdProceeding.ProceedingId }, createdProceeding);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/Proceeding/5
        [HttpGet("{proceedingId}")]
        public async Task<IActionResult> GetProceeding(int proceedingId)
        {
            var proceeding = await _proceedingRepository.GetProceedingByIdAsync(proceedingId);
            if (proceeding == null)
            {
                return NotFound("Proceeding not found.");
            }

            var responseDto = new ProceedingResponseDto
            {
                ProceedingId = proceeding.ProceedingId,
                Title = proceeding.Title,
                Description = proceeding.Description,
                FilePath = proceeding.FilePath,
                Doi = proceeding.Doi,
                Status = proceeding.Status,
                Version = proceeding.Version,
                PublishedDate = proceeding.PublishedDate,
                PublishedBy = proceeding.PublishedByNavigation != null ? new UserInfoDto
                {
                    UserId = proceeding.PublishedByNavigation.UserId,
                    FullName = proceeding.PublishedByNavigation.Name
                } : null,
                Papers = proceeding.Papers?.Select(p => new PaperInfoDto
                {
                    PaperId = p.PaperId,
                    Title = p.Title
                }).ToList()
            };

            return Ok(responseDto);
        }

        // GET: api/Proceeding/papers/10
        [HttpGet("papers/{conferenceId}")]
        public async Task<IActionResult> GetPublishedPapers(int conferenceId)
        {
            var papers = await _proceedingRepository.GetPublishedPapersByConferenceAsync(conferenceId);
            if (papers == null || !papers.Any())
            {
                return NotFound("No published papers found for this conference.");
            }

            var paperDtos = papers.Select(p => new PaperInfoDto
            {
                PaperId = p.PaperId,
                Title = p.Title
            }).ToList();

            return Ok(paperDtos);
        }

        // PUT: api/Proceeding/update/5
        [HttpPut("update/{proceedingId}")]
        public async Task<IActionResult> UpdateProceeding(int proceedingId, [FromBody] ProceedingUpdateDto dto)
        {
            var existingProceeding = await _proceedingRepository.GetProceedingByIdAsync(proceedingId);
            if (existingProceeding == null)
            {
                return NotFound("Proceeding not found.");
            }

            existingProceeding.Title = dto.Title ?? existingProceeding.Title;
            existingProceeding.Description = dto.Description ?? existingProceeding.Description;
            existingProceeding.FilePath = dto.FilePath ?? existingProceeding.FilePath;
            existingProceeding.UpdatedAt = DateTime.UtcNow;
            existingProceeding.Status = dto.Status ?? existingProceeding.Status;
            existingProceeding.Version = dto.Version ?? existingProceeding.Version;
            existingProceeding.Doi = dto.Doi ?? existingProceeding.Doi;

            try
            {
                await _proceedingRepository.UpdateProceedingAsync(existingProceeding);
                return Ok(existingProceeding);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}