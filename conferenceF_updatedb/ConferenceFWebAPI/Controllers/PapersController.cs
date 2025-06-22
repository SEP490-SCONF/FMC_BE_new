using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs;
using ConferenceFWebAPI.Service;
using DataAccess;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace ConferenceFWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PapersController : ControllerBase
    {
        private readonly IPaperRepository _paperRepo;
        private readonly IMapper _mapper;
        private readonly GoogleDriveService _googleDriveService;

        public PapersController(IPaperRepository paperRepo, IMapper mapper, GoogleDriveService googleDriveService)
        {
            _paperRepo = paperRepo;
            _mapper = mapper;
            _googleDriveService = googleDriveService;
        }

        /// <summary>
        /// Submit paper with file upload
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SubmitPaper([FromForm] PaperCreateDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("PDF file is required.");

            string fileUrl = await _googleDriveService.UploadFileAsync(dto.File);

            var paper = _mapper.Map<Paper>(dto);
            paper.FilePath = fileUrl;
            paper.Status = "Submitted";
            paper.SubmitDate = DateTime.UtcNow;
            paper.IsPublished = false;

            // 👇 Chuyển thành danh sách 1 tác giả
            await _paperRepo.AddAsync(paper, new List<int> { dto.AuthorId });

            return Ok(new { message = "Paper submitted successfully.", fileUrl });
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var papers = await _paperRepo.GetAllAsync();
            var result = _mapper.Map<List<PaperDto>>(papers);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var paper = await _paperRepo.GetByIdAsync(id);
            if (paper == null)
                return NotFound();

            var result = _mapper.Map<PaperDto>(paper);
            return Ok(result);
        }
    }
}
