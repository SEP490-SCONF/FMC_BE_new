using AutoMapper;
using ConferenceFWebAPI.DTOs.UserProfile;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace ConferenceFWebAPI.Controllers.UserProfiles
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserProfileController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }



        [HttpPut("{id}/profile")]
        public async Task<IActionResult> UpdateUserProfile(int id, [FromBody] UpdateUserProfile dto)
        {
            try
            {
                var existingUser = await _userRepository.GetById(id);
                if (existingUser == null)
                {
                    return NotFound($"User with ID {id} not found.");
                }

                // Chỉ update những field có trong DTO
                if (!string.IsNullOrWhiteSpace(dto.Name))
                    existingUser.Name = dto.Name;
                if (!string.IsNullOrWhiteSpace(dto.AvatarUrl))
                    existingUser.AvatarUrl = dto.AvatarUrl;

                await _userRepository.Update(existingUser);

                return NoContent(); // hoặc return Ok(existingUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<UserProfile>> GetUserById(int id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null) return NotFound();

            var userDto = _mapper.Map<UserProfile>(user);
            return Ok(userDto);
        }
    }
}
