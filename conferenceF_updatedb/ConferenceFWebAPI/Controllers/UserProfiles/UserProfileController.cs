using AutoMapper;
using ConferenceFWebAPI.DTOs.UserProfile;
using ConferenceFWebAPI.Service; // 👈 Dịch vụ upload Azure Blob Storage
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Repository;

namespace ConferenceFWebAPI.Controllers.UserProfiles
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly IConfiguration _configuration;

        public UserProfileController(
            IUserRepository userRepository,
            IMapper mapper,
            IAzureBlobStorageService azureBlobStorageService,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _azureBlobStorageService = azureBlobStorageService;
            _configuration = configuration;
        }

        // ✅ GIỮ LẠI GET PROFILE
        [HttpGet("{id}")]
        public async Task<ActionResult<UserProfile>> GetUserById(int id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null) return NotFound();

            var userDto = _mapper.Map<UserProfile>(user);
            return Ok(userDto);
        }

        // ✅ UPDATE PROFILE (Name + Avatar)
        [HttpPut("{id}/profile")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateUserProfile(int id, [FromForm] UpdateUserProfile dto)
        {
            try
            {
                var existingUser = await _userRepository.GetById(id);
                if (existingUser == null)
                    return NotFound($"User with ID {id} not found.");

                // Cập nhật tên nếu có
                if (!string.IsNullOrWhiteSpace(dto.Name))
                    existingUser.Name = dto.Name;

                // Upload avatar nếu có file
                if (dto.AvatarFile != null && dto.AvatarFile.Length > 0)
                {
                    // Xóa file cũ nếu có
                    if (!string.IsNullOrEmpty(existingUser.AvatarUrl))
                    {
                        await _azureBlobStorageService.DeleteFileAsync(existingUser.AvatarUrl);
                    }

                    // Lấy container từ config
                    var containerName = _configuration.GetValue<string>("BlobContainers:Avatars");
                    var newAvatarUrl = await _azureBlobStorageService.UploadFileAsync(dto.AvatarFile, containerName);
                    existingUser.AvatarUrl = newAvatarUrl;
                }

                await _userRepository.Update(existingUser);
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi server khi cập nhật user:");
                Console.WriteLine(ex.ToString());
                return StatusCode(500, "Error while updating user");
            }
        }
    }
}
