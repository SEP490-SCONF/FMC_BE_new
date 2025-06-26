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
        private readonly IConferenceRoleRepository _conferenceRoleRepository;

        public UserConferenceRolesController(
            IUserConferenceRoleRepository repo,
            IUserRepository userRepository,
            IEmailService emailService,
            IConferenceRepository conferenceRepository,
            IConferenceRoleRepository conferenceRoleRepository)
        {
            _repo = repo;
            _userRepository = userRepository;
            _emailService = emailService;
            _conferenceRepository = conferenceRepository;
            _conferenceRoleRepository = conferenceRoleRepository;
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
            // Kiểm tra User - Conference - Role đã tồn tại chưa
            var existing = await _repo.GetByCondition(x =>
                x.UserId == dto.UserId &&
                x.ConferenceId == dto.ConferenceId &&
                x.ConferenceRoleId == dto.ConferenceRoleId);

            if (existing.Any())
            {
                return Ok("Người dùng đã được gán vai trò này trong hội thảo.");
            }

            // Kiểm tra dữ liệu liên quan
            var user = await _userRepository.GetById(dto.UserId);
            if (user == null)
                return BadRequest("User không tồn tại.");

            var conference = await _conferenceRepository.GetById(dto.ConferenceId);
            if (conference == null)
                return BadRequest("Hội thảo không tồn tại.");

            var role = await _conferenceRoleRepository.GetById(dto.ConferenceRoleId);
            if (role == null)
                return BadRequest("Vai trò không tồn tại.");

            // Tạo mới quan hệ User - Conference - Role
            var entity = new UserConferenceRole
            {
                UserId = dto.UserId,
                ConferenceRoleId = dto.ConferenceRoleId,
                ConferenceId = dto.ConferenceId,
                AssignedAt = DateTime.UtcNow,            
               
            };

            await _repo.Add(entity);

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

            return Ok("Success");
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UserConferenceRole role)
        {
            if (id != role.Id) return BadRequest("ID mismatch.");
            await _repo.Update(role);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.Delete(id);
            return NoContent();
        }

    }
}
