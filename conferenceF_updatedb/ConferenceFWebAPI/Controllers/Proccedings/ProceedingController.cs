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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // ✅ Kiểm tra xem conference này đã có proceeding chưa
            var existingProceeding = await _proceedingRepository.GetProceedingByConferenceIdAsync(dto.ConferenceId);
            if (existingProceeding != null)
            {
                return Conflict(new
                {
                    Message = "A proceeding for this conference already exists.",
                    ProceedingId = existingProceeding.ProceedingId,
                    Title = existingProceeding.Title
                });
            }

            // Xử lý chuỗi PaperIds
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

        [HttpGet("papers/{conferenceId}")]
        public async Task<IActionResult> GetPublishedPapers(int conferenceId)
        {
            // Include PublishedByNavigation và Papers
            var proceeding = await _proceedingRepository.GetProceedingByConferenceIdAsync(conferenceId);

            if (proceeding == null)
                return NotFound("No proceeding found for this conference.");

            // Lọc các bài báo đã published và accepted
            var publishedPapers = await _proceedingRepository.GetPublishedPapersByConferenceAsync(conferenceId);


            if (publishedPapers == null || !publishedPapers.Any())
                return NotFound("No published papers found for this conference.");

            // Map sang DTO
            var proceedingDto = new ProceedingResponseDto
            {
                ProceedingId = proceeding.ProceedingId,
                Title = proceeding.Conference?.Title,
                Description = proceeding.Conference?.Description,
                FilePath = proceeding.FilePath,
                Doi = proceeding.Doi,
                Status = proceeding.Status,
                Version = proceeding.Version,
                PublishedDate = proceeding.PublishedDate,
                PublishedBy = proceeding.PublishedByNavigation != null
                    ? new UserInfoDto
                    {
                        UserId = proceeding.PublishedByNavigation.UserId,
                        FullName = proceeding.PublishedByNavigation.Name
                    }
                    : null,
                Papers = publishedPapers.Select(p => new PaperInfoDto
                {
                    PaperId = p.PaperId,
                    Title = p.Title,
                    FilePath = p.FilePath
                }).ToList()
            };

            return Ok(proceedingDto);
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
        public async Task<IActionResult> UpdateProceeding(int proceedingId, [FromForm] ProceedingUpdateFromFormDto dto)
        {
            // 1. Kiểm tra tính hợp lệ và sự tồn tại của kỷ yếu
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingProceeding = await _proceedingRepository.GetProceedingByIdAsync(proceedingId);
            if (existingProceeding == null)
            {
                return NotFound("Proceeding not found.");
            }

            // Lưu các URL cũ để xóa sau này
            var oldProceedingFilePath = existingProceeding.FilePath;
            var oldCoverPageUrl = existingProceeding.CoverPageUrl;

            // Xử lý chuỗi PaperIds thành danh sách số nguyên
            var paperIds = new List<int>();
            if (!string.IsNullOrEmpty(dto.PaperIds))
            {
                paperIds = dto.PaperIds
                               .Split(',')
                               .Select(id => int.Parse(id.Trim()))
                               .ToList();
            }

            // 2. Cập nhật các trường dữ liệu và tăng phiên bản
            if (!string.IsNullOrEmpty(dto.Title))
                existingProceeding.Title = dto.Title;

            if (!string.IsNullOrEmpty(dto.Description))
                existingProceeding.Description = dto.Description;

            if (dto.PublishedBy.HasValue)
                existingProceeding.PublishedBy = dto.PublishedBy;

            if (!string.IsNullOrEmpty(dto.Doi))
                existingProceeding.Doi = dto.Doi;

            existingProceeding.PublishedDate = DateTime.UtcNow;
            existingProceeding.Status = "Published";

            // Tăng phiên bản (version)
            if (float.TryParse(existingProceeding.Version, out float currentVersion))
            {
                existingProceeding.Version = (currentVersion + 1.0).ToString("F1");
            }
            else
            {
                existingProceeding.Version = "1.0"; // Fallback nếu phiên bản không hợp lệ
            }

            try
            {
                // 3. Xử lý file PDF mới
                string? newCoverPageUrl = oldCoverPageUrl;
                if (dto.CoverImageFile != null && dto.CoverImageFile.Length > 0)
                {
                    string containerName = "conference-banners";
                    newCoverPageUrl = await _azureBlobStorageService.UploadFileAsync(dto.CoverImageFile, containerName);

                    // ⚠️ Gán lại URL mới cho entity
                    existingProceeding.CoverPageUrl = newCoverPageUrl;
                }


                // Xử lý danh sách PaperIds
                List<Paper> papers = new List<Paper>();
                if (!string.IsNullOrEmpty(dto.PaperIds))
                {
                    var parsedPaperIds = dto.PaperIds   // đổi tên từ paperIds -> parsedPaperIds
                                          .Split(',')
                                          .Select(idStr => int.Parse(idStr.Trim()))
                                          .ToList();

                    papers = await _paperRepository.GetPapersByIdsAsync(parsedPaperIds);
                    existingProceeding.Papers = papers; // cập nhật lại danh sách papers
                }
                else
                {
                    papers = existingProceeding.Papers.ToList(); // giữ nguyên nếu không có PaperIds mới
                }



                var paperFileUrls = papers.Select(p => p.FilePath).ToList();
                var paperTitles = papers.Select(p => p.Title).ToList();

                // Hợp nhất file PDF mới
                byte[] mergedPdfBytes = await _pdfMerger.MergePdfsFromAzureStorageAsync(newCoverPageUrl, paperFileUrls, paperTitles);

                if (mergedPdfBytes == null || mergedPdfBytes.Length == 0)
                {
                    return StatusCode(500, "PDF merge failed, resulting in an empty file.");
                }

                // 4. Tải file mới và xóa file cũ
                string proceedingContainerName = "proceedings";
                string proceedingFileName = $"proceeding_{existingProceeding.ConferenceId}_v{existingProceeding.Version.Replace(".", "_")}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

                using (var stream = new MemoryStream(mergedPdfBytes))
                {
                    existingProceeding.FilePath = await _azureBlobStorageService.UploadStreamAsync(stream, proceedingFileName, proceedingContainerName, "application/pdf");
                }

                // Xóa file cũ sau khi đã upload file mới thành công
                if (!string.IsNullOrEmpty(oldProceedingFilePath))
                {
                    await _azureBlobStorageService.DeleteFileAsync(oldProceedingFilePath);
                }
                if (!string.IsNullOrEmpty(oldCoverPageUrl) && !oldCoverPageUrl.Equals(newCoverPageUrl))
                {
                    await _azureBlobStorageService.DeleteFileAsync(oldCoverPageUrl);
                }

                // 5. Lưu vào cơ sở dữ liệu
                var updatedProceeding = await _proceedingRepository.UpdateProceedingAsync(existingProceeding);

                // Chuẩn bị DTO trả về
                var responseDto = new ProceedingResponseDto
                {
                    ProceedingId = updatedProceeding.ProceedingId,
                    Title = updatedProceeding.Title,
                    Description = updatedProceeding.Description,
                    FilePath = updatedProceeding.FilePath,
                    Doi = updatedProceeding.Doi,
                    Status = updatedProceeding.Status,
                    Version = updatedProceeding.Version,
                    PublishedDate = updatedProceeding.PublishedDate,
                    PublishedBy = updatedProceeding.PublishedByNavigation == null ? null : new UserInfoDto
                    {
                        UserId = updatedProceeding.PublishedByNavigation.UserId,
                        FullName = updatedProceeding.PublishedByNavigation.Name
                    },

                    Papers = updatedProceeding.Papers?.Select(p => new PaperInfoDto
                    {
                        PaperId = p.PaperId,
                        Title = p.Title
                    }).ToList()
                };


                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred in UpdateProceeding: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllProceedings()
        {
            try
            {
                var proceedings = await _proceedingRepository.GetAllProceedingsAsync();

                if (proceedings == null || !proceedings.Any())
                    return NotFound("No proceedings found.");

                var responseDtos = proceedings.Select(p => new ProceedingResponseDto
                {
                    ProceedingId = p.ProceedingId,
                    Title = p.Conference != null ? p.Conference.Title : p.Title,
                    Description = p.Conference != null ? p.Conference.Description : p.Description,
                    FilePath = p.FilePath,
                    CoverPageUrl = p.CoverPageUrl,
                    Doi = p.Doi,
                    Status = p.Status,
                    Version = p.Version,
                    PublishedDate = p.PublishedDate,
                    PublishedBy = p.PublishedByNavigation != null
        ? new UserInfoDto
        {
            UserId = p.PublishedByNavigation.UserId,
            FullName = p.PublishedByNavigation.Name
        }
        : null,
                    Papers = p.Papers?.Select(pa => new PaperInfoDto
                    {
                        PaperId = pa.PaperId,
                        Title = pa.Title
                    }).ToList()
                }).ToList();

                return Ok(responseDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred in GetAllProceedings: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}