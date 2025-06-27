using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.UserConferenceRoles;
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

    }
}
