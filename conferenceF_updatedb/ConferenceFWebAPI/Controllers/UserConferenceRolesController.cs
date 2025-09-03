using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs;
using ConferenceFWebAPI.DTOs.Conferences;
using ConferenceFWebAPI.DTOs.UserConferenceRoles;
using ConferenceFWebAPI.DTOs.UserProfile;
using ConferenceFWebAPI.Service;
using DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.Repository;
using static ConferenceFWebAPI.DTOs.UserConferenceRoles.CommiteeDto;

namespace ConferenceFWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserConferenceRolesController : ControllerBase
    {
        private readonly IUserConferenceRoleRepository _repo;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IConferenceRepository _conferenceRepository;
        private readonly IConferenceRoleRepository _conferenceRoleRepository;
        private readonly AutoMapper.IMapper _mapper; // Cần inject AutoMapper nếu bạn muốn map User entity sang UserDto
        private readonly IConfiguration _configuration;
        private readonly IReviewerAssignmentRepository _reviewerAssignmentRepository;


        public UserConferenceRolesController(
            IUserConferenceRoleRepository repo,
            IUserRepository userRepository,
            IEmailService emailService,
            IConferenceRepository conferenceRepository,
            IConferenceRoleRepository conferenceRoleRepository,
            AutoMapper.IMapper mapper,
            IConfiguration configuration,
            IReviewerAssignmentRepository reviewerAssignmentRepository)
        {
            _repo = repo;
            _userRepository = userRepository;
            _emailService = emailService;
            _conferenceRepository = conferenceRepository;
            _conferenceRoleRepository = conferenceRoleRepository;
            _mapper = mapper;
            _configuration = configuration;
            _reviewerAssignmentRepository = reviewerAssignmentRepository;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _repo.GetAll();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _repo.GetById(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("conference/{conferenceId}")]
        public async Task<IActionResult> GetByConferenceId(int conferenceId)
        {
            var entities = await _repo.GetByConferenceId(conferenceId);

            var result = entities.Select(ucr => new UserConferenceRoleViewDto
            {
                Id = ucr.Id,
                UserId = ucr.UserId,
                UserName = ucr.User.Name,
                UserEmail = ucr.User.Email,
                AvatarUrl = ucr.User.AvatarUrl,
                ConferenceRoleId = ucr.ConferenceRoleId,
                RoleName = ucr.ConferenceRole.RoleName,
                ConferenceId = ucr.ConferenceId,
                ConferenceTitle = ucr.Conference.Title,
                AssignedAt = ucr.AssignedAt
            }).ToList();

            return Ok(result);
        }



        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserConferenceRoleCreateDto dto)
        {
            // 1. Kiểm tra User - Conference - Role đã tồn tại chưa
            var existing = await _repo.GetByCondition(x =>
                x.UserId == dto.UserId &&
                x.ConferenceId == dto.ConferenceId &&
                x.ConferenceRoleId == dto.ConferenceRoleId);

            if (existing.Any())
            {
                return Ok("Người dùng đã được gán vai trò này trong hội thảo.");
            }

            // 2. Kiểm tra dữ liệu liên quan
            var user = await _userRepository.GetById(dto.UserId);
            if (user == null)
                return BadRequest("User không tồn tại.");

            var conference = await _conferenceRepository.GetById(dto.ConferenceId);
            if (conference == null)
                return BadRequest("Hội thảo không tồn tại.");

            var role = await _conferenceRoleRepository.GetById(dto.ConferenceRoleId);
            if (role == null)
                return BadRequest("Vai trò không tồn tại.");

            // 3. Xác định GroupName và SpecificTitle mặc định dựa trên role
            string groupName;
            string specificTitle;

            // Ví dụ đơn giản: nếu role chứa "Reviewer" thì là Scientific Committee, còn lại là Organizing
            if (role.RoleName != null && role.RoleName.ToLower().Contains("reviewer"))
            {
                groupName = "Scientific Committee";
                specificTitle = "Reviewer";
            }
            else if (role.RoleName != null && (role.RoleName.ToLower().Contains("chair") || role.RoleName.ToLower().Contains("organizer")))
            {
                groupName = "Organizing Committee";
                // tách rõ Conference Chair vs Co-Chair nếu cần
                specificTitle = role.RoleName; // có thể là "Conference Chair" hoặc "Co-Chair"
            }
            else
            {
                // fallback chung
                groupName = "Organizing Committee";
                specificTitle = role.RoleName ?? "Member";
            }

            // 4. Sinh token + expires để mời hoàn thiện hồ sơ
            var token = Guid.NewGuid().ToString("N");
            var expires = DateTime.UtcNow.AddDays(7);
            var formBaseUrl = _configuration["CommitteeFormBaseUrl"] ?? throw new InvalidOperationException("CommitteeFormBaseUrl not configured");

            // 5. Tạo mới quan hệ User - Conference - Role
            var entity = new UserConferenceRole
            {
                UserId = dto.UserId,
                ConferenceRoleId = dto.ConferenceRoleId,
                ConferenceId = dto.ConferenceId,
                AssignedAt = DateTime.UtcNow,
                GroupName = groupName,
                SpecificTitle = specificTitle,
                ConfirmationToken = token,
                ExpiresAt = expires,
                IsPublic = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.Add(entity);

            // 6. Tạo link hoàn thiện hồ sơ
            var link = $"{formBaseUrl}?eid={entity.Id}&token={token}&expires={expires:O}";

            // 7. Chuẩn bị nội dung email
            string subject = $"Invitation to Complete Your '{entity.SpecificTitle}' Profile for the '{conference.Title}' Conference";
            string body = $@"
       <h3>Dear {user.Name ?? user.Email},</h3>
<p>You have been assigned the role of <strong>{entity.SpecificTitle}</strong> for the <strong>{conference.Title}</strong> conference (as part of the {entity.GroupName}).</p>
<p>Please click the link below to complete your personal information:</p>
<p><a href='{link}' target='_blank'>Complete Your Profile</a></p>
<p>This link will expire on: {expires:dd/MM/yyyy HH:mm} (UTC)</p>
<br/>
<p>Sincerely,<br/>The Organizing Committee</p>";

            await _emailService.SendEmailAsync(user.Email, subject, body);

            return Ok("Success");
        }

        [HttpGet("conference/{conferenceId}/roles/members")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetConferenceMembersByRoles(int conferenceId)
        {
            try
            {
                // Kiểm tra sự tồn tại của Conference
                var conference = await _conferenceRepository.GetById(conferenceId);
                if (conference == null)
                {
                    return NotFound($"Hội thảo với ID: {conferenceId} không tồn tại.");
                }

                // Định nghĩa các vai trò cần tìm (3 hoặc 4)
                var desiredRoleIds = new List<int> { 3, 4 };

                // Lấy danh sách các User từ Repository
                var users = await _repo.GetUsersByConferenceIdAndRolesAsync(conferenceId, desiredRoleIds);

                if (users == null || !users.Any())
                {
                    return NotFound($"Không tìm thấy người dùng nào với vai trò {string.Join(" hoặc ", desiredRoleIds)} trong hội thảo {conferenceId}.");
                }

                var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);


                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetConferenceMembersByRoles: {ex.ToString()}");
                return StatusCode(500, $"Lỗi nội bộ server khi lấy danh sách thành viên: {ex.Message}");
            }
        }
        [HttpGet("conference/{conferenceId}/roles-reviewer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EnableQuery]
        public async Task<IActionResult> GetConferenceMembersReviewer(
    int conferenceId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 4,
    [FromQuery] string? search = null)
        {
            try
            {
                var roleIds = new List<int> { 3 }; // Reviewer

                var userRoles = (await _repo.GetUsersByConferenceIdAndRolesAsync(conferenceId, roleIds)).ToList();

                // Optional search filter
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower().Trim();
                    userRoles = userRoles.Where((UserConferenceRole u) =>
    (!string.IsNullOrEmpty(u.User?.Name) && u.User.Name.ToLower().Contains(search)) ||
    (!string.IsNullOrEmpty(u.User?.Email) && u.User.Email.ToLower().Contains(search))
).ToList();

                }

                var totalCount = userRoles.Count;

                var pagedResult = userRoles
                    .OrderByDescending(u => u.AssignedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(ucr => new UserConferenceRoleViewDto
                    {
                        Id = ucr.Id,
                        UserId = ucr.UserId,
                        UserName = ucr.User?.Name ?? "",
                        UserEmail = ucr.User?.Email ?? "",
                        AvatarUrl = ucr.User?.AvatarUrl,
                        ConferenceRoleId = ucr.ConferenceRoleId,
                        RoleName = ucr.ConferenceRole?.RoleName ?? "",
                        ConferenceId = ucr.ConferenceId,
                        ConferenceTitle = ucr.Conference?.Title ?? "",
                        AssignedAt = ucr.AssignedAt
                    })
                    .ToList();

                // Thêm total count vào header để frontend đọc được
                Response.Headers.Add("X-Total-Count", totalCount.ToString());

                return Ok(new
                {
                    value = pagedResult,
                    @odata_count = totalCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("change-role")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangeUserConferenceRole([FromBody] UserConferenceRoleChangeRoleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Kiểm tra sự tồn tại của User, Conference và NewConferenceRole
                var user = await _userRepository.GetById(dto.UserId);
                if (user == null)
                    return BadRequest("User không tồn tại.");

                var conference = await _conferenceRepository.GetById(dto.ConferenceId);
                if (conference == null)
                    return BadRequest("Hội thảo không tồn tại.");

                var newRole = await _conferenceRoleRepository.GetById(dto.NewConferenceRoleId);
                if (newRole == null)
                    return BadRequest("Vai trò mới không tồn tại.");

                var updatedAssignment = await _repo.UpdateConferenceRoleForUserInConference(dto.UserId, dto.ConferenceId, dto.NewConferenceRoleId);

                if (updatedAssignment == null)
                {
                    return NotFound($"Không tìm thấy bản ghi vai trò cho người dùng {dto.UserId} trong hội thảo {dto.ConferenceId} để cập nhật.");
                }

                string subject = $"Your Role in the '{conference.Title}' Conference Has Been Updated";
                string body = $@"
                    <h3>Dear {user.Name},</h3>
<p>Your role in the <strong>{conference.Title}</strong> conference has been updated to <strong>{newRole.RoleName}</strong>.</p>
<p>Update Time: {DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm")} UTC</p>
<p>Please log in to the system to view more details.</p>
<br/>
<p>Sincerely,<br/>The Organizing Committee</p>";

                // Gửi email
                await _emailService.SendEmailAsync(user.Email, subject, body);

                return Ok($"Vai trò của người dùng {dto.UserId} trong hội thảo {dto.ConferenceId} đã được thay đổi thành {newRole.RoleName}.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi nội bộ server khi thay đổi vai trò: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.Delete(id);
            return NoContent();
        }
        [HttpGet("user/{userId}/conferences/{roleName}")]
        [ProducesResponseType(typeof(List<ConferenceDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetConferencesByUserAndRole(int userId, string roleName)
        {
            var conferences = await _repo.GetConferencesByUserIdAndRoleAsync(userId, roleName);

            if (conferences == null || !conferences.Any())
                return NotFound($"No conferences found for user {userId} with role {roleName}.");

            var result = conferences.Select(c => new OrganizerConferenceDTO
            {
                ConferenceId = c.ConferenceId,
                Title = c.Title,
                Description = c.Description,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Location = c.Location,
                Status = c.Status,
                BannerImage = c.BannerUrl  
            }).ToList();

            return Ok(result);
        }
        [HttpPost("create-or-assign")]
        public async Task<IActionResult> CreateOrAssign([FromBody] AddOrganizerDto dto)
        {
            // 1. Kiểm tra hoặc tạo user
            var user = await _userRepository.GetByEmail(dto.Email);
            if (user == null)
            {
                user = new User
                {
                    Email = dto.Email,
                    CreatedAt = DateTime.UtcNow,
                    RoleId = 2, // Role mặc định
                    Status = true
                };
                await _userRepository.Add(user);
            }

            // 2. Kiểm tra quan hệ đã tồn tại chưa
            var existing = await _repo.GetByCondition(x =>
                x.UserId == user.UserId &&
                x.ConferenceId == dto.ConferenceId &&
                x.ConferenceRoleId == 4); // Organizer

            if (existing.Any())
            {
                return Ok("Người dùng đã được gán vai trò này trong hội thảo.");
            }

            // 3. Kiểm tra dữ liệu liên quan
            var foundUser = await _userRepository.GetById(user.UserId);
            if (foundUser == null)
                return BadRequest("User không tồn tại.");

            var conference = await _conferenceRepository.GetById(dto.ConferenceId);
            if (conference == null)
                return BadRequest("Hội thảo không tồn tại.");

            var role = await _conferenceRoleRepository.GetById(4);
            if (role == null)
                return BadRequest("Vai trò không tồn tại.");

            // 4. Tạo token và link form
            var token = Guid.NewGuid().ToString("N");
            var expires = DateTime.UtcNow.AddDays(7); // Hết hạn sau 7 ngày
            var formBaseUrl = _configuration["CommitteeFormBaseUrl"];

            // 5. Tạo mới UserConferenceRole
            var entity = new UserConferenceRole
            {
                UserId = foundUser.UserId,
                ConferenceRoleId = 4, // Organizer
                ConferenceId = dto.ConferenceId,
                GroupName = "Organizing Committee",
                SpecificTitle = string.IsNullOrWhiteSpace(dto.SpecificTitle) ? "Organizer" : dto.SpecificTitle,
                ConfirmationToken = token,
                ExpiresAt = expires,
                IsPublic = false,
                AssignedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.Add(entity);

            // 6. Sinh link form mời điền thông tin
            var link = $"{formBaseUrl}?eid={entity.Id}&token={token}&expires={expires:O}";

            // 7. Email mời điền form
            string subject = $"Invitation to Complete Your '{entity.SpecificTitle}' Profile for the '{conference.Title}' Conference'";
            string body = $@"
<h3>Dear {foundUser.Name ?? foundUser.Email},</h3>
<p>You have been assigned the role of <strong>{entity.SpecificTitle}</strong> for the <strong>{conference.Title}</strong> conference.</p>
<p>Please click the link below to complete your personal information:</p>
<p><a href='{link}' target='_blank'>Complete Your Profile</a></p>
<p>This link will expire on: {expires:dd/MM/yyyy HH:mm} (UTC)</p>
<br/>
<p>Sincerely,<br/>The Organizing Committee</p>";

            await _emailService.SendEmailAsync(foundUser.Email, subject, body);

            return Ok("Success");
        }
        [HttpGet("{id}/form")]

        public async Task<IActionResult> GetCommitteeForm(int id, [FromQuery] string token)
        {
            var entity = await _repo.GetById(id);
            if (entity == null) return NotFound();

            if (entity.ConfirmationToken != token || entity.ExpiresAt < DateTime.UtcNow)
                return BadRequest("Invalid or expired token.");

            return Ok(new
            {
                groupName = entity.GroupName,
                specificTitle = entity.SpecificTitle,
                affiliation = entity.Affiliation,
                expertise = entity.Expertise,
                displayNameOverride = entity.DisplayNameOverride,
                isPublic = entity.IsPublic
            });
        }
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteCommitteeForm(int id, [FromBody] CompleteCommitteeDto dto)
        {
            var entity = await _repo.GetById(id);
            if (entity == null) return NotFound();

            if (entity.ConfirmationToken != dto.Token || entity.ExpiresAt < DateTime.UtcNow)
                return BadRequest("Invalid or expired token.");

            entity.GroupName = dto.GroupName;
            entity.SpecificTitle = dto.SpecificTitle;
            entity.Affiliation = dto.Affiliation;
            entity.Expertise = dto.Expertise;
            entity.DisplayNameOverride = dto.DisplayNameOverride;

            // Khi submit là confirm => public luôn
            entity.IsPublic = true;
            entity.ConfirmedAt = DateTime.UtcNow;
            entity.ConfirmationToken = null; // Disable reuse

            await _repo.Update(entity);

            return Ok("Information updated successfully");
        }
        [HttpGet("conference/{conferenceId}/committee")]
        public async Task<IActionResult> GetCommitteeByConference(int conferenceId)
        {
            var conference = await _conferenceRepository.GetById(conferenceId);
            if (conference == null)
                return NotFound($"Hội thảo với ID {conferenceId} không tồn tại.");

            var assignments = (await _repo.GetByConferenceId(conferenceId))
                .Where(ucr => ucr.IsPublic)
                .ToList();

            var grouped = assignments
                .OrderBy(u => u.GroupName)
                .ThenBy(u => u.SortOrder ?? int.MaxValue)
                .ThenBy(u => u.SpecificTitle)
                .GroupBy(u => string.IsNullOrWhiteSpace(u.GroupName) ? "Ungrouped" : u.GroupName!)
                .Select(g => new CommitteeGroupDto
                {
                    GroupName = g.Key,
                    Members = g.Select(ucr => new CommitteeMemberDto
                    {
                        UserId = ucr.UserId,
                        DisplayName = string.IsNullOrWhiteSpace(ucr.DisplayNameOverride)
                            ? (ucr.User?.Name ?? "")
                            : ucr.DisplayNameOverride!,
                        Email = ucr.User?.Email,
                        AvatarUrl = ucr.User?.AvatarUrl,
                        RoleName = ucr.ConferenceRole?.RoleName,
                        SpecificTitle = ucr.SpecificTitle,
                        Affiliation = ucr.Affiliation,
                        Expertise = ucr.Expertise,
                        ConfirmedAt = ucr.ConfirmedAt
                    }).ToList()
                })
                .ToList();

            var result = new CommitteeViewDto
            {
                ConferenceId = conference.ConferenceId,
                ConferenceTitle = conference.Title ?? "",
                Groups = grouped
            };

            return Ok(result);
        }
        [HttpGet("reviewers/{reviewerId}/assigned-paper-count")]
        public async Task<IActionResult> GetReviewerAssignedPaperCount(int conferenceId, int reviewerId)
        {
            try
            {
                var count = await _reviewerAssignmentRepository
                    .GetAssignedPaperCountByReviewerIdAndConferenceId(reviewerId, conferenceId);

                return Ok(new { ReviewerId = reviewerId, ConferenceId = conferenceId, AssignedPaperCount = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("resolve-authors")]
        public async Task<IActionResult> ResolveAuthors([FromBody] AuthorEmailsDto dto)
        {
            if (dto.Emails == null || !dto.Emails.Any())
                return BadRequest("Email list cannot be empty.");

            var userIds = new List<int>();

            foreach (var email in dto.Emails.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(email))
                    continue;

                // 1. Kiểm tra user có tồn tại chưa
                var user = await _userRepository.GetByEmail(email);
                if (user == null)
                {
                    // Nếu chưa có -> tạo user mới
                    user = new User
                    {
                        Email = email.Trim(),
                        CreatedAt = DateTime.UtcNow,
                        RoleId = 2, // Default: Author
                        Status = true
                    };
                    await _userRepository.Add(user);
                }

                userIds.Add(user.UserId);
            }

            return Ok(userIds);
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(List<UserConferenceRoleViewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var entities = await _repo.GetByUserId(userId);

            if (entities == null || !entities.Any())
                return NotFound($"No conference roles found for user ID: {userId}");

            var result = entities.Select(ucr => new UserConferenceRoleViewDto
            {
                Id = ucr.Id,
                UserId = ucr.UserId,
                UserName = ucr.User.Name,
                UserEmail = ucr.User.Email,
                ConferenceRoleId = ucr.ConferenceRoleId,
                RoleName = ucr.ConferenceRole.RoleName,
                ConferenceId = ucr.ConferenceId,
                ConferenceTitle = ucr.Conference.Title,
                AvatarUrl = ucr.User.AvatarUrl,
                AssignedAt = ucr.AssignedAt
            }).ToList();

            return Ok(result);
        }

        [HttpGet("available-users")]
        public async Task<IActionResult> GetAvailableUsers()
        {
            var users = await _repo.GetUsersWithoutAnyConferenceRole();

            var result = users.Select(u => new
            {
                u.UserId,
                u.Name,
                u.Email,
                u.AvatarUrl
            });

            return Ok(result);
        }
    }
}
