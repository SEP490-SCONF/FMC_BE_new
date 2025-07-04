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


        public UserConferenceRolesController(
            IUserConferenceRoleRepository repo,
            IUserRepository userRepository,
            IEmailService emailService,
            IConferenceRepository conferenceRepository,
            IConferenceRoleRepository conferenceRoleRepository,
            AutoMapper.IMapper mapper)
        {
            _repo = repo;
            _userRepository = userRepository;
            _emailService = emailService;
            _conferenceRepository = conferenceRepository;
            _conferenceRoleRepository = conferenceRoleRepository;
            _mapper = mapper;
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
            var result = await _repo.GetByConferenceId(conferenceId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserConferenceRoleCreateDto dto)
        {
            // Kiểm tra xem người dùng đã tồn tại hay chưa
            var user = await _userRepository.GetByEmail(dto.Email);
            if (user == null)
            {
                // Nếu không tồn tại, tạo người dùng mới
                user = new User
                {
                    Email = dto.Email,
                    CreatedAt = DateTime.UtcNow,
                    RoleId = 2, // Gán role mặc định, bạn có thể thay đổi nếu cần
                    Status = true // Status mặc định
                };

                // Thêm người dùng vào cơ sở dữ liệu
                await _userRepository.Add(user);
            }

            // Kiểm tra xem User - Conference - Role đã tồn tại chưa
            var existing = await _repo.GetByCondition(x =>
                x.UserId == user.UserId &&
                x.ConferenceId == dto.ConferenceId &&
                x.ConferenceRoleId == 4); // Kiểm tra role với ConferenceRoleId = 4

            if (existing.Any())
            {
                return Ok("Người dùng đã được gán vai trò này trong hội thảo.");
            }

            // Kiểm tra dữ liệu liên quan (User, Conference, Role)
            var foundUser = await _userRepository.GetById(user.UserId);
            if (foundUser == null)
                return BadRequest("User không tồn tại.");

            var conference = await _conferenceRepository.GetById(dto.ConferenceId);
            if (conference == null)
                return BadRequest("Hội thảo không tồn tại.");

            var role = await _conferenceRoleRepository.GetById(4); // Kiểm tra vai trò với ConferenceRoleId = 4
            if (role == null)
                return BadRequest("Vai trò không tồn tại.");

            // Tạo mới quan hệ User - Conference - Role
            var entity = new UserConferenceRole
            {
                UserId = foundUser.UserId,
                ConferenceRoleId = 4, // Vai trò được gán
                ConferenceId = dto.ConferenceId,
                AssignedAt = DateTime.UtcNow,
            };

            await _repo.Add(entity);

            // Chuẩn bị nội dung email
            string subject = $"Bạn đã được gán vai trò '{role.RoleName}' trong hội thảo '{conference.Title}'";
            string body = $@"
        <h3>Xin chào {foundUser.Name},</h3>
        <p>Bạn vừa được gán vai trò <strong>{role.RoleName}</strong> trong hội thảo <strong>{conference.Title}</strong>.</p>
        <p>Thời gian gán: {DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm")} UTC</p>
        <p>Vui lòng đăng nhập hệ thống để theo dõi thông tin chi tiết.</p>
        <br/>
        <p>Trân trọng,<br/>Ban tổ chức</p>";

            // Gửi email
            await _emailService.SendEmailAsync(foundUser.Email, subject, body);

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
        public async Task<IActionResult> GetConferenceMembersReviewer(int conferenceId)
        {
            try
            {
                // Kiểm tra sự tồn tại của Conference
                var conference = await _conferenceRepository.GetById(conferenceId);
                if (conference == null)
                {
                    return NotFound($"Hội thảo với ID: {conferenceId} không tồn tại.");
                }

                var desiredRoleIds = new List<int> { 3 };

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

                string subject = $"Vai trò của bạn đã được cập nhật trong hội thảo '{conference.Title}'";
                string body = $@"
                    <h3>Xin chào {user.Name},</h3>
                    <p>Vai trò của bạn trong hội thảo <strong>{conference.Title}</strong> đã được cập nhật thành <strong>{newRole.RoleName}</strong>.</p>
                    <p>Thời gian cập nhật: {DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm")} UTC</p>
                    <p>Vui lòng đăng nhập hệ thống để theo dõi thông tin chi tiết.</p>
                    <br/>
                    <p>Trân trọng,<br/>Ban tổ chức</p>";

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

            var conferenceDtos = _mapper.Map<List<ConferenceDTO>>(conferences);
            return Ok(conferenceDtos);
        }



    }
}
