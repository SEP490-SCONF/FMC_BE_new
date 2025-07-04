using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository;
using System.Threading.Tasks;

namespace ConferenceFWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CRUDUserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public CRUDUserController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepository.GetAll();
            var result = _mapper.Map<IEnumerable<UserInformationDTO>>(users);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromForm] UpdateUserDTO dto)
        {
            var existingUser = await _userRepository.GetById(id);
            if (existingUser == null)
                return NotFound($"User with ID {id} not found.");

            if (!string.IsNullOrWhiteSpace(dto.Name))
                existingUser.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.AvatarUrl))
                existingUser.AvatarUrl = dto.AvatarUrl;

            if (dto.RoleId.HasValue)
            {
                var roleExists = await _userRepository.RoleExists(dto.RoleId.Value);
                if (!roleExists)
                    return BadRequest($"RoleId {dto.RoleId.Value} does not exist.");

                existingUser.RoleId = dto.RoleId.Value;
            }

            if (dto.Status.HasValue)
            {
                existingUser.Status = dto.Status.Value;
            }

            await _userRepository.Update(existingUser);

            var updatedUser = await _userRepository.GetById(id);
            var result = _mapper.Map<UserInformationDTO>(updatedUser);

            return Ok(result);
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

            var userProfile = _mapper.Map<UserInformationDTO>(addedUser);

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

            var userProfile = _mapper.Map<UserInformationDTO>(user);
            return Ok(userProfile);
        }
    }
}
