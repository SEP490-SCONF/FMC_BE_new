using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Repository;
using System.Threading.Tasks;

namespace ConferenceFWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddUserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public AddUserController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpPost("email")]
        public async Task<IActionResult> CreateUserByEmail([FromBody] AddUserByEmailDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest("Email is required.");
            }

            var existingUser = await _userRepository.GetByEmail(dto.Email);
            if (existingUser != null)
            {
                return Conflict($"User with email {dto.Email} already exists.");
            }

            var newUser = new User
            {
                Email = dto.Email,
                RoleId = 2, 
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.Add(newUser);

            var addedUser = await _userRepository.GetById(newUser.UserId);

            var userProfile = _mapper.Map<UserProfile>(addedUser);

            return CreatedAtAction(nameof(GetUserById), new { id = addedUser.UserId }, userProfile);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null)
            {
                return NotFound();
            }

            var userProfile = _mapper.Map<UserProfile>(user);
            return Ok(userProfile);
        }
    }
}
