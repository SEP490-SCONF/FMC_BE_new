using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.Certificates;
using ConferenceFWebAPI.Service;
using Microsoft.AspNetCore.Mvc;
using Repository;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing;

namespace ConferenceFWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificatesController : ControllerBase
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly IRegistrationRepository _registrationRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaperRepository _paperRepository;
        private readonly IUserConferenceRoleRepository _userConferenceRoleRepository;
        private readonly IMapper _mapper;
        private readonly IAzureBlobStorageService _azureBlobStorageService;

        public CertificatesController(
            ICertificateRepository certificateRepository,
            IRegistrationRepository registrationRepository,
            IPaymentRepository paymentRepository,
            IPaperRepository paperRepository,
            IUserConferenceRoleRepository userConferenceRoleRepository,
            IMapper mapper,
            IAzureBlobStorageService azureBlobStorageService)
        {
            _certificateRepository = certificateRepository;
            _registrationRepository = registrationRepository;
            _paymentRepository = paymentRepository;
            _paperRepository = paperRepository;
            _userConferenceRoleRepository = userConferenceRoleRepository;
            _mapper = mapper;
            _azureBlobStorageService = azureBlobStorageService;
        }

        // GET: api/Certificates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CertificateDto>>> GetCertificates()
        {
            try
            {
                var certificates = await _certificateRepository.GetAll();
                var certificateDtos = new List<CertificateDto>();

                foreach (var c in certificates)
                {
                    // Đảm bảo lấy đầy đủ dữ liệu từ context
                    var userName = c.Reg?.User?.Name;
                    var userEmail = c.Reg?.User?.Email;
                    var conferenceTitle = c.Reg?.Conference?.Title;
                    var conferenceRoleName = c.UserConferenceRole?.ConferenceRole?.RoleName;

                    // Nếu navigation properties null, có thể cần lazy loading hoặc explicit loading
                    if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(conferenceTitle))
                    {
                        // Log warning for missing data
                        Console.WriteLine($"Warning: Missing navigation data for Certificate ID {c.CertificateId}");
                    }

                    certificateDtos.Add(new CertificateDto
                    {
                        CertificateId = c.CertificateId,
                        RegId = c.RegId,
                        IssueDate = c.IssueDate,
                        CertificateUrl = c.CertificateUrl,
                        CertificateNumber = c.CertificateNumber,
                        Status = c.Status,
                        CreatedAt = c.CreatedAt,
                        UserConferenceRoleId = c.UserConferenceRoleId,
                        UserName = userName ?? "Unknown User",
                        UserEmail = userEmail ?? "Unknown Email",
                        ConferenceTitle = conferenceTitle ?? "Unknown Conference",
                        ConferenceRoleName = conferenceRoleName ?? "Unknown Role"
                    });
                }

                return Ok(certificateDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Certificates/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CertificateDto>> GetCertificate(int id)
        {
            try
            {
                var certificate = await _certificateRepository.GetById(id);
                if (certificate == null)
                {
                    return NotFound($"Certificate with ID {id} not found.");
                }

                var certificateDto = new CertificateDto
                {
                    CertificateId = certificate.CertificateId,
                    RegId = certificate.RegId,
                    IssueDate = certificate.IssueDate,
                    CertificateUrl = certificate.CertificateUrl,
                    CertificateNumber = certificate.CertificateNumber,
                    Status = certificate.Status,
                    CreatedAt = certificate.CreatedAt,
                    UserConferenceRoleId = certificate.UserConferenceRoleId,
                    UserName = certificate.Reg?.User?.Name,
                    UserEmail = certificate.Reg?.User?.Email,
                    ConferenceTitle = certificate.Reg?.Conference?.Title,
                    ConferenceRoleName = certificate.UserConferenceRole?.ConferenceRole?.RoleName
                };

                return Ok(certificateDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Certificates/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<CertificateDto>>> GetCertificatesByUserId(int userId)
        {
            try
            {
                var certificates = await _certificateRepository.GetByUserId(userId);
                var certificateDtos = certificates.Select(c => new CertificateDto
                {
                    CertificateId = c.CertificateId,
                    RegId = c.RegId,
                    IssueDate = c.IssueDate,
                    CertificateUrl = c.CertificateUrl,
                    CertificateNumber = c.CertificateNumber,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt,
                    UserConferenceRoleId = c.UserConferenceRoleId,
                    UserName = c.Reg?.User?.Name,
                    UserEmail = c.Reg?.User?.Email,
                    ConferenceTitle = c.Reg?.Conference?.Title,
                    ConferenceRoleName = c.UserConferenceRole?.ConferenceRole?.RoleName
                }).ToList();

                return Ok(certificateDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Certificates/conference/5
        [HttpGet("conference/{conferenceId}")]
        public async Task<ActionResult<IEnumerable<CertificateDto>>> GetCertificatesByConferenceId(int conferenceId)
        {
            try
            {
                var certificates = await _certificateRepository.GetByConferenceId(conferenceId);
                var certificateDtos = certificates.Select(c => new CertificateDto
                {
                    CertificateId = c.CertificateId,
                    RegId = c.RegId,
                    IssueDate = c.IssueDate,
                    CertificateUrl = c.CertificateUrl,
                    CertificateNumber = c.CertificateNumber,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt,
                    UserConferenceRoleId = c.UserConferenceRoleId,
                    UserName = c.Reg?.User?.Name,
                    UserEmail = c.Reg?.User?.Email,
                    ConferenceTitle = c.Reg?.Conference?.Title,
                    ConferenceRoleName = c.UserConferenceRole?.ConferenceRole?.RoleName
                }).ToList();

                return Ok(certificateDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Certificates/generate-for-paper
        [HttpPost("generate-for-paper")]
        public async Task<ActionResult<IEnumerable<CertificateDto>>> GenerateCertificatesForApprovedPaper([FromBody] int paperId)
        {
            try
            {
                // Lấy thông tin paper với đầy đủ navigation properties
                var paper = await _paperRepository.GetPaperByIdWithIncludesAsync(paperId);
                if (paper == null)
                {
                    return NotFound("Paper not found.");
                }

                // Kiểm tra paper đã được approve/publish chưa (chấp nhận cả "Approved" và "Accepted")
                if (paper.Status != "Approved" && paper.Status != "Accepted" && paper.Status != "Published")
                {
                    return BadRequest($"Paper status is '{paper.Status}'. Only 'Approved', 'Accepted' or 'Published' papers can generate certificates.");
                }

                var generatedCertificates = new List<CertificateDto>();

                // Lấy tất cả authors của paper thông qua PaperAuthor
                var paperAuthors = paper.PaperAuthors?.ToList();
                if (paperAuthors == null || !paperAuthors.Any())
                {
                    return BadRequest("No authors found for this paper.");
                }

                foreach (var paperAuthor in paperAuthors)
                {
                    try
                    {
                        // Lấy thông tin author đầy đủ từ database nếu navigation property null
                        var authorUser = paperAuthor.Author;
                        if (authorUser == null)
                        {
                            // Fallback: lấy thông tin user từ repository nếu navigation property không được load
                            var userRepository = HttpContext.RequestServices.GetService<IUserRepository>();
                            if (userRepository != null)
                            {
                                authorUser = await userRepository.GetById(paperAuthor.AuthorId);
                            }
                        }

                        // Lấy thông tin conference đầy đủ từ database nếu navigation property null
                        var conference = paper.Conference;
                        if (conference == null)
                        {
                            var conferenceRepository = HttpContext.RequestServices.GetService<IConferenceRepository>();
                            if (conferenceRepository != null)
                            {
                                conference = await conferenceRepository.GetById(paper.ConferenceId);
                            }
                        }

                        // Tìm registration của author cho conference này
                        var registrations = await _registrationRepository.GetByUserId(paperAuthor.AuthorId);
                        var registration = registrations.FirstOrDefault(r => r.ConferenceId == paper.ConferenceId);

                        if (registration == null)
                        {
                            // Tạo registration tạm thời cho author nếu chưa có
                            var newRegistration = new Registration
                            {
                                UserId = paperAuthor.AuthorId,
                                ConferenceId = paper.ConferenceId,
                                RegisteredAt = DateTime.UtcNow
                            };
                            await _registrationRepository.Add(newRegistration);
                            registration = newRegistration;
                        }

                        // Kiểm tra xem đã có certificate cho registration này chưa
                        var existingCertificate = await _certificateRepository.GetByRegistrationId(registration.RegId);
                        
                        if (existingCertificate != null)
                        {
                            // Nếu đã có certificate, thêm vào kết quả
                            generatedCertificates.Add(new CertificateDto
                            {
                                CertificateId = existingCertificate.CertificateId,
                                RegId = existingCertificate.RegId,
                                IssueDate = existingCertificate.IssueDate,
                                CertificateUrl = existingCertificate.CertificateUrl,
                                CertificateNumber = existingCertificate.CertificateNumber,
                                Status = existingCertificate.Status,
                                CreatedAt = existingCertificate.CreatedAt,
                                UserConferenceRoleId = existingCertificate.UserConferenceRoleId,
                                UserName = authorUser?.Name ?? "Unknown Author",
                                UserEmail = authorUser?.Email ?? "Unknown Email",
                                ConferenceTitle = conference?.Title ?? "Unknown Conference",
                                ConferenceRoleName = "Author"
                            });
                            continue;
                        }

                        // Tìm hoặc tạo UserConferenceRole với role Author
                        var userConferenceRoles = await _userConferenceRoleRepository.GetByConferenceId(paper.ConferenceId);
                        var authorRole = userConferenceRoles.FirstOrDefault(ucr => 
                            ucr.UserId == paperAuthor.AuthorId && 
                            ucr.ConferenceRole.RoleName == "Author");

                        int? userConferenceRoleId = authorRole?.Id;

                        // Tạo certificate mới
                        var certificate = new Certificate
                        {
                            RegId = registration.RegId,
                            IssueDate = DateTime.UtcNow,
                            CertificateNumber = GenerateCertificateNumber(),
                            Status = true,
                            CreatedAt = DateTime.UtcNow,
                            UserConferenceRoleId = userConferenceRoleId
                        };

                        await _certificateRepository.Add(certificate);

                        // Generate certificate image
                        var imageBytes = GenerateCertificateImage(
                            certificate, 
                            authorUser?.Name ?? "Unknown Author", 
                            conference?.Title ?? "Unknown Conference", 
                            paper.Title ?? "Unknown Paper"
                        );

                        // Upload certificate image to Azure Blob Storage
                        var fileName = $"certificate_{certificate.CertificateId}_{certificate.CertificateNumber}.png";
                        var certificateUrl = await _azureBlobStorageService.UploadImageAsync(
                            imageBytes, 
                            fileName, 
                            "certificates"
                        );
                        
                        // Update certificate URL
                        certificate.CertificateUrl = certificateUrl;
                        await _certificateRepository.Update(certificate);

                        // Thêm vào kết quả
                        generatedCertificates.Add(new CertificateDto
                        {
                            CertificateId = certificate.CertificateId,
                            RegId = certificate.RegId,
                            IssueDate = certificate.IssueDate,
                            CertificateUrl = certificate.CertificateUrl,
                            CertificateNumber = certificate.CertificateNumber,
                            Status = certificate.Status,
                            CreatedAt = certificate.CreatedAt,
                            UserConferenceRoleId = certificate.UserConferenceRoleId,
                            UserName = authorUser?.Name ?? "Unknown Author",
                            UserEmail = authorUser?.Email ?? "Unknown Email",
                            ConferenceTitle = conference?.Title ?? "Unknown Conference",
                            ConferenceRoleName = "Author"
                        });
                    }
                    catch (Exception authorEx)
                    {
                        // Log lỗi cho author cụ thể nhưng tiếp tục với authors khác
                        Console.WriteLine($"Error generating certificate for author {paperAuthor.AuthorId}: {authorEx.Message}");
                    }
                }

                if (!generatedCertificates.Any())
                {
                    return BadRequest("No certificates could be generated for this paper's authors.");
                }

                return Ok(generatedCertificates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Certificates/generate
        [HttpPost("generate")]
        public async Task<ActionResult<CertificateDto>> GenerateCertificate([FromBody] CertificateGenerateDto dto)
        {
            try
            {
                // Kiểm tra registration tồn tại
                var registration = await _registrationRepository.GetById(dto.RegId);
                if (registration == null)
                {
                    return BadRequest("Registration not found.");
                }

                // Kiểm tra payment đã hoàn thành
                var isPaymentCompleted = await _certificateRepository.IsPaymentCompleted(dto.RegId);
                if (!isPaymentCompleted)
                {
                    return BadRequest("Payment must be completed before generating certificate.");
                }

                // Tạo certificate
                var certificate = await _certificateRepository.GenerateCertificate(dto.RegId);

                var certificateDto = new CertificateDto
                {
                    CertificateId = certificate.CertificateId,
                    RegId = certificate.RegId,
                    IssueDate = certificate.IssueDate,
                    CertificateUrl = certificate.CertificateUrl,
                    CertificateNumber = certificate.CertificateNumber,
                    Status = certificate.Status,
                    CreatedAt = certificate.CreatedAt,
                    UserConferenceRoleId = certificate.UserConferenceRoleId
                };

                return Ok(certificateDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Certificates/5/url
        [HttpGet("{id}/url")]
        public async Task<ActionResult<object>> GetCertificateUrl(int id)
        {
            try
            {
                var certificate = await _certificateRepository.GetById(id);
                if (certificate == null)
                {
                    return NotFound("Certificate not found.");
                }

                // Nếu certificate chưa có URL hoặc URL không hợp lệ, tạo và upload lên Azure Blob Storage
                if (string.IsNullOrEmpty(certificate.CertificateUrl) || !certificate.CertificateUrl.StartsWith("https://"))
                {
                    // Lấy thông tin đầy đủ từ context
                    var userName = certificate.Reg?.User?.Name ?? "Unknown Author";
                    var conferenceTitle = certificate.Reg?.Conference?.Title ?? "Unknown Conference";
                    var paperTitle = "Research Paper"; // Default value
                    
                    // Tạo certificate image
                    var imageBytes = GenerateCertificateImage(certificate, userName, conferenceTitle, paperTitle);
                    
                    // Upload lên Azure Blob Storage
                    var fileName = $"certificate_{certificate.CertificateId}_{certificate.CertificateNumber}.png";
                    var certificateUrl = await _azureBlobStorageService.UploadImageAsync(
                        imageBytes, 
                        fileName, 
                        "certificates"
                    );
                    
                    // Update certificate URL in database
                    certificate.CertificateUrl = certificateUrl;
                    await _certificateRepository.Update(certificate);
                }

                return Ok(new { certificateUrl = certificate.CertificateUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Certificates/5/download-image
        [HttpGet("{id}/download-image")]
        public async Task<IActionResult> DownloadCertificateImage(int id)
        {
            try
            {
                var certificate = await _certificateRepository.GetById(id);
                if (certificate == null)
                {
                    return NotFound("Certificate not found.");
                }

                // Lấy thông tin đầy đủ từ context
                var userName = certificate.Reg?.User?.Name ?? "Unknown Author";
                var conferenceTitle = certificate.Reg?.Conference?.Title ?? "Unknown Conference";
                
                // Lấy paper title từ registration context
                var paperTitle = "Research Paper"; // Default value
                
                // Tạo certificate image
                var imageBytes = GenerateCertificateImage(certificate, userName, conferenceTitle, paperTitle);

                // Nếu certificate chưa có URL hoặc URL không hợp lệ, upload lên Azure Blob Storage
                if (string.IsNullOrEmpty(certificate.CertificateUrl) || !certificate.CertificateUrl.StartsWith("https://"))
                {
                    var fileName = $"certificate_{certificate.CertificateId}_{certificate.CertificateNumber}.png";
                    var certificateUrl = await _azureBlobStorageService.UploadImageAsync(
                        imageBytes, 
                        fileName, 
                        "certificates"
                    );
                    
                    // Update certificate URL in database
                    certificate.CertificateUrl = certificateUrl;
                    await _certificateRepository.Update(certificate);
                }

                var fileName2 = $"certificate_{certificate.CertificateNumber}.png";
                return File(imageBytes, "image/png", fileName2);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Certificates/5/download
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadCertificate(int id)
        {
            try
            {
                var certificate = await _certificateRepository.GetById(id);
                if (certificate == null)
                {
                    return NotFound("Certificate not found.");
                }

                var pdfBytes = await _certificateRepository.GenerateCertificatePdf(id);
                var fileName = $"certificate_{certificate.CertificateNumber}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Certificates/verify
        [HttpPost("verify")]
        public async Task<ActionResult<CertificateVerifyDto>> VerifyCertificate([FromBody] string certificateNumber)
        {
            try
            {
                var certificates = await _certificateRepository.GetAll();
                var certificate = certificates.FirstOrDefault(c => c.CertificateNumber == certificateNumber);

                if (certificate == null)
                {
                    return Ok(new CertificateVerifyDto
                    {
                        CertificateNumber = certificateNumber,
                        IsValid = false,
                        VerificationMessage = "Certificate not found."
                    });
                }

                // Tính toán lại blockchain hash để verify
                var expectedHash = GenerateBlockchainHash(certificate);
                
                return Ok(new CertificateVerifyDto
                {
                    CertificateNumber = certificateNumber,
                    BlockchainHash = expectedHash,
                    IsValid = true,
                    VerificationMessage = $"Certificate is valid. Issued to {certificate.Reg?.User?.Name} for {certificate.Reg?.Conference?.Title}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Certificates/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCertificate(int id, [FromBody] CertificateUpdateDto dto)
        {
            try
            {
                if (id != dto.CertificateId)
                {
                    return BadRequest("Certificate ID mismatch.");
                }

                var certificate = await _certificateRepository.GetById(id);
                if (certificate == null)
                {
                    return NotFound("Certificate not found.");
                }

                // Update fields
                if (!string.IsNullOrEmpty(dto.CertificateUrl))
                    certificate.CertificateUrl = dto.CertificateUrl;
                
                if (dto.Status.HasValue)
                    certificate.Status = dto.Status.Value;
                
                if (dto.UserConferenceRoleId.HasValue)
                    certificate.UserConferenceRoleId = dto.UserConferenceRoleId.Value;

                await _certificateRepository.Update(certificate);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/Certificates/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCertificate(int id)
        {
            try
            {
                var certificate = await _certificateRepository.GetById(id);
                if (certificate == null)
                {
                    return NotFound("Certificate not found.");
                }

                await _certificateRepository.Delete(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private string GenerateCertificateNumber()
        {
            // Tạo số certificate unique dựa trên timestamp và random
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"CERT-{timestamp}-{random}";
        }

        private byte[] GenerateCertificateImage(Certificate certificate, string userName, string conferenceTitle, string paperTitle)
        {
            try
            {
                const int width = 1200;
                const int height = 850;
                
                using (var image = new Image<Rgba32>(width, height))
                {
                    // Background với màu chủ đạo của FPT
                    image.Mutate(ctx =>
                    {
                        // Nền trắng chính
                        ctx.Fill(Color.White);
                        
                        // Viền ngoài màu cam FPT
                        ctx.Draw(Color.FromRgb(255, 102, 0), 6, new Rectangle(15, 15, width - 30, height - 30));
                        
                        // Viền trong màu xanh navy FPT
                        ctx.Draw(Color.FromRgb(0, 51, 102), 3, new Rectangle(35, 35, width - 70, height - 70));
                        
                        // Header background với gradient FPT colors
                        var headerRect = new Rectangle(50, 50, width - 100, 120);
                        ctx.Fill(Color.FromRgba(0, 51, 102, 20), headerRect);
                    });
                    
                    // Fonts theo phong cách FPT
                    var logoFont = SystemFonts.CreateFont("Arial", 16, FontStyle.Bold);
                    var titleFont = SystemFonts.CreateFont("Times New Roman", 42, FontStyle.Bold);
                    var subtitleFont = SystemFonts.CreateFont("Times New Roman", 22);
                    var nameFont = SystemFonts.CreateFont("Times New Roman", 38, FontStyle.Bold);
                    var bodyFont = SystemFonts.CreateFont("Times New Roman", 18);
                    var smallFont = SystemFonts.CreateFont("Arial", 12);
                    var signatureFont = SystemFonts.CreateFont("Times New Roman", 14, FontStyle.Italic);
                    
                    image.Mutate(ctx =>
                    {
                        // Logo/Header FPT University
                        var logoOptions = new RichTextOptions(logoFont)
                        {
                            Origin = new PointF(width / 2, 85),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        ctx.DrawText(logoOptions, "FPT UNIVERSITY", Color.FromRgb(255, 102, 0));
                        
                        var subLogoOptions = new RichTextOptions(SystemFonts.CreateFont("Arial", 12))
                        {
                            Origin = new PointF(width / 2, 105),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        ctx.DrawText(subLogoOptions, "Excellence in Education - Innovation in Research", Color.FromRgb(0, 51, 102));
                        
                        // Đường phân cách
                        var separator = new RectangularPolygon(200, 139, width - 400, 2);
                        ctx.Fill(Color.FromRgb(255, 102, 0), separator);
                        
                        // Title chính
                        var titleOptions = new RichTextOptions(titleFont)
                        {
                            Origin = new PointF(width / 2, 200),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        ctx.DrawText(titleOptions, "CERTIFICATE", Color.FromRgb(0, 51, 102));
                        
                        // Subtitle
                        var subtitleOptions = new RichTextOptions(subtitleFont)
                        {
                            Origin = new PointF(width / 2, 240),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        ctx.DrawText(subtitleOptions, "of Academic Achievement", Color.FromRgb(102, 102, 102));
                        
                        // Body text với phong cách học thuật
                        var bodyOptions = new RichTextOptions(bodyFont)
                        {
                            Origin = new PointF(width / 2, 300),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        ctx.DrawText(bodyOptions, "This is to certify that", Color.FromRgb(51, 51, 51));
                        
                        // Tên người nhận - highlight
                        var nameOptions = new RichTextOptions(nameFont)
                        {
                            Origin = new PointF(width / 2, 350),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        ctx.DrawText(nameOptions, userName.ToUpper(), Color.FromRgb(255, 102, 0));
                        
                        
                        // Achievement text
                        var achievementOptions = new RichTextOptions(bodyFont)
                        {
                            Origin = new PointF(width / 2, 420),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        ctx.DrawText(achievementOptions, "has successfully presented a research paper at", Color.FromRgb(51, 51, 51));
                        
                        // Conference title - emphasized
                        var conferenceOptions = new RichTextOptions(SystemFonts.CreateFont("Times New Roman", 20, FontStyle.Bold))
                        {
                            Origin = new PointF(width / 2, 460),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        ctx.DrawText(conferenceOptions, conferenceTitle, Color.FromRgb(0, 51, 102));
                        
                        // Paper title với format academic
                        var paperOptions = new RichTextOptions(bodyFont)
                        {
                            Origin = new PointF(width / 2, 520),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            WrappingLength = width - 200
                        };
                        ctx.DrawText(paperOptions, $"Research Title: \"{paperTitle}\"", Color.FromRgb(102, 102, 102));
                        
                        // Recognition text
                        var recognitionOptions = new RichTextOptions(bodyFont)
                        {
                            Origin = new PointF(width / 2, 580),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        ctx.DrawText(recognitionOptions, "and is hereby recognized for outstanding academic contribution", Color.FromRgb(51, 51, 51));
                        
                        // Signature section
                        var signatureY = 650;
                        
                        // Left signature
                        var leftSigOptions = new RichTextOptions(signatureFont)
                        {
                            Origin = new PointF(200, signatureY),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        ctx.DrawText(leftSigOptions, "Conference Chair", Color.FromRgb(102, 102, 102));
                        var leftSigLine = new RectangularPolygon(120, signatureY - 21, 160, 1);
                        ctx.Fill(Color.FromRgb(102, 102, 102), leftSigLine);
                        
                        // Right signature  
                        var rightSigOptions = new RichTextOptions(signatureFont)
                        {
                            Origin = new PointF(width - 200, signatureY),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        ctx.DrawText(rightSigOptions, "Dean of Research", Color.FromRgb(102, 102, 102));
                        var rightSigLine = new RectangularPolygon(width - 280, signatureY - 21, 160, 1);
                        ctx.Fill(Color.FromRgb(102, 102, 102), rightSigLine);
                        
                        // Footer information
                        var footerY = height - 100;
                        
                        // Date
                        var dateOptions = new RichTextOptions(smallFont)
                        {
                            Origin = new PointF(80, footerY)
                        };
                        ctx.DrawText(dateOptions, $"Date Issued: {certificate.IssueDate:dd/MM/yyyy}", Color.FromRgb(102, 102, 102));
                        
                        // Certificate number
                        var certNumberOptions = new RichTextOptions(smallFont)
                        {
                            Origin = new PointF(width - 280, footerY)
                        };
                        ctx.DrawText(certNumberOptions, $"Certificate No: FPT-{certificate.CertificateNumber}", Color.FromRgb(102, 102, 102));
                        
                        // QR code placeholder text
                        var qrOptions = new RichTextOptions(SystemFonts.CreateFont("Arial", 10))
                        {
                            Origin = new PointF(width / 2, footerY + 20),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        ctx.DrawText(qrOptions, "Verify at: verify.fpt.edu.vn", Color.FromRgb(153, 153, 153));
                        
                        // Blockchain hash - smaller and less prominent
                        var hashOptions = new RichTextOptions(SystemFonts.CreateFont("Courier New", 9))
                        {
                            Origin = new PointF(width / 2, height - 25),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        ctx.DrawText(hashOptions, $"Blockchain: {GenerateBlockchainHash(certificate)}", Color.FromRgb(204, 204, 204));
                        
                        // Decorative elements - corners
                        DrawDecorativeCorner(ctx, 70, 70, Color.FromRgb(255, 102, 0));
                        DrawDecorativeCorner(ctx, width - 70, 70, Color.FromRgb(255, 102, 0), true);
                        DrawDecorativeCorner(ctx, 70, height - 70, Color.FromRgb(0, 51, 102), false, true);
                        DrawDecorativeCorner(ctx, width - 70, height - 70, Color.FromRgb(0, 51, 102), true, true);
                    });
                    
                    // Convert to PNG
                    using (var stream = new MemoryStream())
                    {
                        image.SaveAsPng(stream);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating FPT certificate: {ex.Message}");
                return GenerateFallbackCertificate(userName, ex.Message);
            }
        }

        // Helper method để vẽ góc trang trí
        private void DrawDecorativeCorner(IImageProcessingContext ctx, float x, float y, Color color, 
            bool flipX = false, bool flipY = false)
        {
            var size = 20;
            var offsetX = flipX ? -size : 0;
            var offsetY = flipY ? -size : 0;
            
            // Vẽ các đường trang trí góc bằng hình chữ nhật nhỏ
            var horizontalLine = new RectangularPolygon(x + offsetX, y + offsetY - 1, flipX ? -size : size, 2);
            ctx.Fill(color, horizontalLine);
            
            var verticalLine = new RectangularPolygon(x + offsetX - 1, y + offsetY, 2, flipY ? -size : size);
            ctx.Fill(color, verticalLine);
        }

        // Fallback method cải tiến
        private byte[] GenerateFallbackCertificate(string userName, string errorMessage)
        {
            try
            {
                using (var fallbackImage = new Image<Rgba32>(800, 600))
                {
                    fallbackImage.Mutate(ctx =>
                    {
                        ctx.Fill(Color.White);
                        ctx.Draw(Color.FromRgb(255, 102, 0), 3, new Rectangle(10, 10, 780, 580));
                        
                        var titleFont = SystemFonts.CreateFont("Arial", 28, FontStyle.Bold);
                        var bodyFont = SystemFonts.CreateFont("Arial", 16);
                        
                        var titleOptions = new RichTextOptions(titleFont)
                        {
                            Origin = new PointF(400, 200),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        ctx.DrawText(titleOptions, "FPT UNIVERSITY", Color.FromRgb(0, 51, 102));
                        
                        var nameOptions = new RichTextOptions(SystemFonts.CreateFont("Arial", 24, FontStyle.Bold))
                        {
                            Origin = new PointF(400, 300),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        ctx.DrawText(nameOptions, $"Certificate for: {userName}", Color.FromRgb(255, 102, 0));
                        
                        var errorOptions = new RichTextOptions(bodyFont)
                        {
                            Origin = new PointF(400, 400),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        ctx.DrawText(errorOptions, "Certificate generation temporarily unavailable", Color.FromRgb(153, 153, 153));
                    });
                    
                    using (var stream = new MemoryStream())
                    {
                        fallbackImage.SaveAsPng(stream);
                        return stream.ToArray();
                    }
                }
            }
            catch
            {
                // Ultimate fallback - simple 1x1 pixel PNG
                var simplePng = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChAI/hxyOUwAAAABJRU5ErkJggg==";
                return Convert.FromBase64String(simplePng);
            }
        }
        

        private string GenerateBlockchainHash(Certificate certificate)
        {
            var dataToHash = $"{certificate.RegId}_{certificate.CertificateNumber}_{certificate.IssueDate:yyyyMMddHHmmss}";
            
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }
    }
}
