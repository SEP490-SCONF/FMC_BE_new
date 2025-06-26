using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs.User;
using Google.Apis.Auth;
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
    public class AdminLoginController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AdminLoginController(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> AdminLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string> { _configuration["Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);

                var user = await _userRepository.GetByEmail(payload.Email);

                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "Admin user does not exist." });
                }

                // Phải là role Admin
                if (user.Role?.RoleName != "Admin")
                {
                    return Forbid("You are not authorized to access admin functionality.");
                }

                // Update refresh token
                user.RefreshToken = GenerateRefreshToken();
                user.TokenExpiry = DateTime.UtcNow.AddDays(7);
                await _userRepository.Update(user);
                // Set HttpOnly cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                };
                Response.Cookies.Append("refreshToken", user.RefreshToken, cookieOptions);
                var accessToken = GenerateToken(user);

                var response = new GoogleLoginResponse
                {
                    AccessToken = accessToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    
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
        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString("N");
        }

        private string GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Name ?? ""),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
