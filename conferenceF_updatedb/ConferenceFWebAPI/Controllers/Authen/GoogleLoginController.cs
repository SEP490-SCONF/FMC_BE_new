using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.User;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ConferenceFWebAPI.Controllers.Authen
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleLoginController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public GoogleLoginController(IUserRepository userRepository, IMapper mapper, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _configuration = configuration;
        }
        [HttpPost]
        [Route("Login")]

        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                // Lấy idToken từ yêu cầu gửi đến
                string idToken = request.IdToken;

                // Cấu hình xác thực Google
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { "80241279325-7vv3gco6bt8rdchkh1oepa3hn1mp8bnr.apps.googleusercontent.com" } // Thay bằng Client ID của bạn
                };

                // Xác thực token
                GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);


                var user = await _userRepository.GetByEmail(payload.Email);
                bool isNew = false;

                if (user == null)
                {
                    user = new User
                    {
                        Email = payload.Email,
                        Name = payload.Name,
                        AvatarUrl = payload.Picture,
                        RoleId = 2,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _userRepository.Add(user);
                    isNew = true;
                }
                // 3. Cập nhật refresh token
                user.RefreshToken = GenerateRefreshToken();
                user.TokenExpiry = DateTime.UtcNow.AddDays(7);
                await _userRepository.Update(user);

                // 4. Tạo access token (JWT)
                var accessToken = GenerateToken(user);
                // 5. Chuẩn bị response
                var response = new GoogleLoginResponse
                {
                    AccessToken = accessToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    RefreshToken = user.RefreshToken
                    
                };

                return Ok(response);
            }
            catch (InvalidJwtException ex)
            {
                return BadRequest(new { success = false, message = "Invalid token: " + ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
        [NonAction]
        public string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString("N");
        }

        private string GenerateToken(User user)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "User")
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
               _configuration["Jwt:Issuer"],
               _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1), // Token expiry time (e.g., 1 hour)
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
