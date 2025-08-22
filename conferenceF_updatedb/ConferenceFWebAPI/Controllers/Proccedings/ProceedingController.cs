using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.Proccedings;
using ConferenceFWebAPI.Service;
using iText.Kernel.Utils;
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
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly PdfMergerService _pdfMerger; // Thêm PdfMerger vào đây


        public ProceedingController(IProceedingRepository proceedingRepository, IPaperRepository paperRepository, IUserRepository userRepository, IAzureBlobStorageService azureBlobStorageService,
        PdfMergerService pdfMerger)
        {
            _proceedingRepository = proceedingRepository;
            _paperRepository = paperRepository;
            _userRepository = userRepository;
            _pdfMerger = pdfMerger;
            _azureBlobStorageService = azureBlobStorageService;
        }

        // POST: api/Proceeding/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateProceeding([FromForm] ProceedingCreateFromFormDto dto)
        {
            // Kiểm tra tính hợp lệ của model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kiểm tra xem đã tồn tại kỷ yếu cho hội thảo này chưa
            var existingProceeding = await _proceedingRepository.GetProceedingByConferenceIdAsync(dto.ConferenceId);
            if (existingProceeding != null)
            {
                return BadRequest("A proceeding for this conference already exists.");
            }

            // Xử lý chuỗi PaperIds thành danh sách số nguyên
            var paperIds = new List<int>();
            if (!string.IsNullOrEmpty(dto.PaperIds))
            {
                paperIds = dto.PaperIds
                               .Split(',')
                               .Select(id => int.Parse(id.Trim()))
                               .ToList();
            }

            try
            {
                // 1. Tải lên file ảnh bìa (nếu có) và lấy URL
                string? coverPageUrl = null;
                if (dto.CoverImageFile != null && dto.CoverImageFile.Length > 0)
                {
                    string containerName = "conference-banners";
                    coverPageUrl = await _azureBlobStorageService.UploadFileAsync(dto.CoverImageFile, containerName);
                }

                // 2. Lấy thông tin các bài báo từ cơ sở dữ liệu và sắp xếp theo thứ tự đã nhập
                // 2. Lấy thông tin các bài báo từ cơ sở dữ liệu và sắp xếp theo thứ tự đã nhập
                var papers = new List<Paper>();

                // Sử dụng phương thức GetPapersByIdsAsync để lấy nhiều bài báo
                var foundPapers = await _paperRepository.GetPapersByIdsAsync(paperIds);

                // Sắp xếp các bài báo theo đúng thứ tự của paperIds
                foreach (var paperId in paperIds)
                {
                    var paper = foundPapers.FirstOrDefault(p => p.PaperId == paperId);
                    if (paper != null)
                    {
                        papers.Add(paper);
                    }
                }

                // Nếu không tìm thấy bất kỳ bài báo nào, trả về lỗi
                if (!papers.Any())
                {
                    return BadRequest("No valid papers found to create the proceeding.");
                }

                // Lấy danh sách URL và tiêu đề của các bài báo
                var paperFileUrls = papers.Select(p => p.FilePath).ToList();
                var paperTitles = papers.Select(p => p.Title).ToList();

                // 3. Hợp nhất ảnh bìa, mục lục và các file PDF của bài báo
                byte[] mergedPdfBytes = await _pdfMerger.MergePdfsFromAzureStorageAsync(coverPageUrl, paperFileUrls, paperTitles);

                // 4. Tải file kỷ yếu đã tổng hợp lên Azure Blob Storage
                string proceedingContainerName = "proceedings";
                string proceedingFileName = $"proceeding_{dto.ConferenceId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

                string mergedFileUrl;
                using (var stream = new MemoryStream(mergedPdfBytes))
                {
                    mergedFileUrl = await _azureBlobStorageService.UploadStreamAsync(stream, proceedingFileName, proceedingContainerName, "application/pdf");
                }

                // 5. Tạo đối tượng Proceeding và lưu vào cơ sở dữ liệu
                var newProceeding = new Proceeding
                {
                    ConferenceId = dto.ConferenceId,
                    Title = dto.Title,
                    Description = dto.Description,
                    PublishedBy = dto.PublishedBy,
                    PublishedDate = DateTime.UtcNow,
                    Status = "Published",
                    FilePath = mergedFileUrl,
                    Doi = dto.Doi,
                    Version = "1.0",
                    CoverPageUrl = coverPageUrl,
                    Papers = papers
                };

                foreach (var paper in papers)
                {
                    paper.IsPublished = true;
                }

                var createdProceeding = await _proceedingRepository.CreateProceedingAsync(newProceeding);

                // 6. Chuẩn bị DTO trả về
                var fullProceeding = await _proceedingRepository.GetProceedingByIdAsync(createdProceeding.ProceedingId);

                var responseDto = new ProceedingResponseDto
                {
                    ProceedingId = fullProceeding.ProceedingId,
                    Title = fullProceeding.Title,
                    Description = fullProceeding.Description,
                    FilePath = fullProceeding.FilePath,
                    Doi = fullProceeding.Doi,
                    Status = fullProceeding.Status,
                    Version = fullProceeding.Version,
                    PublishedDate = fullProceeding.PublishedDate,
                    PublishedBy = fullProceeding.PublishedByNavigation != null ? new UserInfoDto
                    {
                        UserId = fullProceeding.PublishedByNavigation.UserId,
                        FullName = fullProceeding.PublishedByNavigation.Name
                    } : null,
                    Papers = fullProceeding.Papers?.Select(p => new PaperInfoDto
                    {
                        PaperId = p.PaperId,
                        Title = p.Title
                    }).ToList()
                };

                return CreatedAtAction(nameof(GetProceeding), new { proceedingId = responseDto.ProceedingId }, responseDto);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi để dễ dàng gỡ lỗi
                Console.WriteLine($"An error occurred in CreateProceeding: {ex.Message}");
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
        [HttpGet("download/{conferenceId}")]
        public async Task<IActionResult> DownloadProceeding(int conferenceId)
        {
            var filePath = await _proceedingRepository.GetFilePathByConferenceIdAsync(conferenceId);

            if (string.IsNullOrEmpty(filePath))
            {
                return NotFound("Proceeding not found for this conference.");
            }

            // Redirect người dùng đến URL của file trong Azure Blob Storage
            return Redirect(filePath);
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