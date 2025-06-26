using AutoMapper;
using ConferenceFWebAPI.DTOs.User;
using DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;
using System.Security.Claims;

namespace ConferenceFWebAPI.Controllers.Authen
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bảo vệ toàn bộ controller hoặc riêng action
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        // GET api/User/Information
        [Authorize]
        [HttpGet("Information")]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Lấy userId từ claim NameIdentifier
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // Lấy dữ liệu user từ repository
            var user = await _userRepository.GetById(userId);
            if (user == null)
                return NotFound();

            // Map sang DTO để trả về
            var userInfomation = _mapper.Map<UserInfomation>(user);
            return Ok(userInfomation);
        }
    }
}
