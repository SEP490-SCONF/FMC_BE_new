using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.Paper;
using ConferenceFWebAPI.Service;
using DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Repository;
using System.Security.Claims;

namespace ConferenceFWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PapersController : ControllerBase
    {
        private readonly IPaperRepository _paperRepository;
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly IConfiguration _configuration;
        private readonly AutoMapper.IMapper _mapper;
        private readonly IUserConferenceRoleRepository _userConferenceRoleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConferenceRepository _conferenceRepository;
        private readonly IConferenceRoleRepository _conferenceRoleRepository;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env; // Inject IWebHostEnvironment


        public PapersController(
            IPaperRepository paperRepository,
            IAzureBlobStorageService azureBlobStorageService,
            IConfiguration configuration,
            AutoMapper.IMapper mapper,
            IUserConferenceRoleRepository userConferenceRoleRepository,
            IUserRepository userRepository,
            IConferenceRepository conferenceRepository,
            IConferenceRoleRepository conferenceRoleRepository,
            IEmailService emailService,
            IWebHostEnvironment env)
        {
            _paperRepository = paperRepository;
            _azureBlobStorageService = azureBlobStorageService;
            _configuration = configuration;
            _mapper = mapper;
            _userConferenceRoleRepository = userConferenceRoleRepository;
            _userRepository = userRepository;
            _conferenceRepository = conferenceRepository;
            _conferenceRoleRepository = conferenceRoleRepository;
            _emailService = emailService;
            _env = env;
        }
        [HttpGet("conference/{conferenceId}")] // Ví dụ: api/PaperByConference/conference/1
        [ProducesResponseType(typeof(List<PaperResponseDto>), StatusCodes.Status200OK)] // Cập nhật kiểu trả về
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetPapersByConference(int conferenceId)
        {
            var papers = _paperRepository.GetPapersByConferenceId(conferenceId);

            if (papers == null || !papers.Any())
            {
                return NotFound($"No papers found for conference ID: {conferenceId}");
            }

            // Ánh xạ danh sách các Paper sang danh sách các PaperResponseDto
            var paperDto = _mapper.Map<List<PaperResponseDto>>(papers);


            return Ok(paperDto);
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

            if (paperDto.AuthorIds == null || !paperDto.AuthorIds.Any())
            {
                return BadRequest("At least one author must be provided.");
            }

            int uploaderUserId;

            if (_env.IsDevelopment())
            {

                uploaderUserId = paperDto.AuthorIds.First(); 
                Console.WriteLine($"[DEVELOPMENT MODE] Inferred UploaderUserId from AuthorIds: {uploaderUserId}");
            }
            else 
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out uploaderUserId))
                {
                    return Unauthorized("User is not authenticated or user ID is not available.");
                }
            }

            if (!paperDto.AuthorIds.Contains(uploaderUserId))
            {
                return BadRequest($"The authenticated user (ID: {uploaderUserId}) must be one of the provided Author IDs.");
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
                int authorOrder = 1;
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

                int conferenceId = paper.ConferenceId;
                int newRoleId = 2; 

                var conference = await _conferenceRepository.GetById(conferenceId);
                var newRole = await _conferenceRoleRepository.GetById(newRoleId);

                if (conference == null || newRole == null)
                {
                    Console.WriteLine($"Warning: Could not update roles for authors in conference {conferenceId}. Missing Conference or Role data. Email skipped.");
                }
                else
                {
                    foreach (var authorId in paperDto.AuthorIds.Distinct()) // Duyệt qua từng tác giả
                    {
                        var authorUser = await _userRepository.GetById(authorId);
                        if (authorUser == null)
                        {
                            Console.WriteLine($"Warning: Author User ID {authorId} not found. Skipping role update and email for this author.");
                            continue; // Bỏ qua tác giả này và tiếp tục với tác giả khác
                        }

                        var updatedRoleAssignment = await _userConferenceRoleRepository.UpdateConferenceRoleForUserInConference(
                            authorId,      // Sử dụng authorId hiện tại trong vòng lặp
                            conferenceId,
                            newRoleId
                        );

                        // Gửi email và log tùy thuộc vào việc vai trò đã được cập nhật hay tạo mới
                        string emailSubject, emailBody;

                        if (updatedRoleAssignment == null)
                        {
                            // Nếu bản ghi UserConferenceRole không tồn tại, tạo mới nó
                            var newAssignment = new UserConferenceRole
                            {
                                UserId = authorId,
                                ConferenceId = conferenceId,
                                ConferenceRoleId = newRoleId,
                                AssignedAt = DateTime.UtcNow
                            };
                            await _userConferenceRoleRepository.Add(newAssignment);
                            Console.WriteLine($"Created new UserConferenceRole for User {authorId} in Conference {conferenceId} with Role {newRole.RoleName}.");

                            emailSubject = $"Vai trò mới của bạn trong hội thảo '{conference.Title}'";
                            emailBody = $@"
                                <h3>Xin chào {authorUser.Name},</h3>
                                <p>Bạn vừa được gán vai trò <strong>{newRole.RoleName}</strong> trong hội thảo <strong>{conference.Title}</strong> vì đã tải lên bài báo.</p>
                                <p>Thời gian gán: {DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm")} UTC</p>
                                <p>Vui lòng đăng nhập hệ thống để theo dõi thông tin chi tiết.</p>
                                <br/>
                                <p>Trân trọng,<br/>Ban tổ chức</p>";

                        }
                        else if (updatedRoleAssignment.ConferenceRoleId != newRoleId)
                        {
                            // Nếu bản ghi tồn tại và vai trò được cập nhật
                            Console.WriteLine($"Updated UserConferenceRole for User {authorId} in Conference {conferenceId} to Role {newRole.RoleName}.");

                            emailSubject = $"Vai trò của bạn đã được cập nhật trong hội thảo '{conference.Title}'";
                            emailBody = $@"
                                <h3>Xin chào {authorUser.Name},</h3>
                                <p>Vai trò của bạn trong hội thảo <strong>{conference.Title}</strong> đã được cập nhật thành <strong>{newRole.RoleName}</strong> do bạn đã tải lên bài báo.</p>
                                <p>Thời gian cập nhật: {DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm")} UTC</p>
                                <p>Vui lòng đăng nhập hệ thống để theo dõi thông tin chi tiết.</p>
                                <br/>
                                <p>Trân trọng,<br/>Ban tổ chức</p>";
                        }
                        else
                        {
                            // Vai trò đã đúng rồi, không cần làm gì
                            Console.WriteLine($"User {authorId} already has Role {newRole.RoleName} in Conference {conferenceId}. No update needed.");
                            continue; // Chuyển sang tác giả tiếp theo mà không gửi email nếu không có thay đổi
                        }

                        // Gửi email cho tác giả hiện tại
                        await _emailService.SendEmailAsync(authorUser.Email, emailSubject, emailBody);
                    }
                }
                // --- END NEW LOGIC ---

                return Ok(new { Message = "File uploaded and paper data saved successfully. User roles updated/assigned.", FileUrl = fileUrl, PaperId = paper.PaperId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during PDF upload or role update: {ex.ToString()}");
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
        [HttpGet("user/{userId}/conference/{conferenceId}")] // Example: api/papers/user/1/conference/10
        [ProducesResponseType(typeof(List<PaperResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Added for invalid input
        public IActionResult GetPapersByUserAndConference(int userId, int conferenceId)
        {
            // Basic validation
            if (userId <= 0 || conferenceId <= 0)
            {
                return BadRequest("User ID and Conference ID must be positive integers.");
            }

            var papers = _paperRepository.GetPapersByUserIdAndConferenceId(userId, conferenceId);

            if (papers == null || !papers.Any())
            {
                return NotFound($"No papers found for User ID: {userId} in Conference ID: {conferenceId}.");
            }

            // Map the list of Paper entities to a list of DTOs
            var paperDtos = _mapper.Map<List<PaperResponseDto>>(papers);

            return Ok(paperDtos);
        }
    }
}
