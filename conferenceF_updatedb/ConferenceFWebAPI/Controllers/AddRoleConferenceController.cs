using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs;
using ConferenceFWebAPI.Service;
using Microsoft.AspNetCore.Mvc;
using Repository;

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
        private readonly IRoleRepository _roleRepository;

        public UserConferenceRolesController(
            IUserConferenceRoleRepository repo,
            IUserRepository userRepository,
            IEmailService emailService,
            IConferenceRepository conferenceRepository,
            IRoleRepository roleRepository)
        {
            _repo = repo;
            _userRepository = userRepository;
            _emailService = emailService;
            _conferenceRepository = conferenceRepository;
            _roleRepository = roleRepository;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _repo.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _repo.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("conference/{conferenceId}")]
        public async Task<IActionResult> GetByConferenceId(int conferenceId)
        {
            var result = await _repo.GetByConferenceIdAsync(conferenceId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserConferenceRoleCreateDto dto)
        {
            // Tạo mới quan hệ User - Conference - Role
            var entity = new UserConferenceRole
            {
                UserId = dto.UserId,
                ConferenceRoleId = dto.RoleId,
                ConferenceId = dto.ConferenceId,
                AssignedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(entity);

            // Lấy thông tin cần thiết
            var user = await _userRepository.GetById(dto.UserId);
            if (user == null)
                return BadRequest("User không tồn tại.");

            var conference = await _conferenceRepository.GetById(dto.ConferenceId);
            if (conference == null)
                return BadRequest("Hội thảo không tồn tại.");

            var role = await _roleRepository.GetById(dto.RoleId);
            if (role == null)
                return BadRequest("Vai trò không tồn tại.");

            // Chuẩn bị nội dung email
            string subject = $"Bạn đã được gán vai trò '{role.RoleName}' trong hội thảo '{conference.Title}'";
            string body = $@"
        <h3>Xin chào {user.Name},</h3>
        <p>Bạn vừa được gán vai trò <strong>{role.RoleName}</strong> trong hội thảo <strong>{conference.Title}</strong>.</p>
        <p>Thời gian gán: {DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm")} UTC</p>
        <p>Vui lòng đăng nhập hệ thống để theo dõi thông tin chi tiết.</p>
        <br/>
        <p>Trân trọng,<br/>Ban tổ chức</p>";

            // Gửi email
            await _emailService.SendEmailAsync(user.Email, subject, body);

            return Ok(entity);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UserConferenceRole role)
        {
            if (id != role.Id) return BadRequest("ID mismatch.");
            await _repo.UpdateAsync(role);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            return NoContent();
        }

    }
}
