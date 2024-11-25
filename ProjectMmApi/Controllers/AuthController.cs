using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using ProjectMmApi.Data;
using ProjectMmApi.Interfaces;
using ProjectMmApi.Models.Dtos;

namespace ProjectMmApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IPasswordHasher _passwordHasher;
        public AuthController(ITokenService tokenService, ApplicationDbContext dbContext,
            IPasswordHasher passwordHasher)
        {
            _tokenService = tokenService;
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
        }

        [HttpPost]
        public IActionResult Login(LoginDto loginDto)
        {
            if (HttpContext.Items.ContainsKey("IsLoggedIn") && HttpContext.Items["IsLoggedIn"] is bool isLoggedIn && isLoggedIn)
            {
                return BadRequest("Already logged in.");
            }

            var user = _dbContext.Users.FirstOrDefault(e => e.Email == loginDto.Email);

            if (user == null)
            {
                return Unauthorized("The email provided is not registered.");
            }

            bool isPasswordCorrect = _passwordHasher.Verify(user.Password, loginDto.Password);

            if (!isPasswordCorrect)
            {
                return Unauthorized("The password entered is incorrect.");
            }

            string accessToken = _tokenService.CreateAccessToken(user);
            string refreshToken = _tokenService.CreateRefreshToken(user);

            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions()
            {
                Expires = DateTimeOffset.Now.AddDays(7),
                HttpOnly = true,
                Secure = true
            });

            return Ok(new
            {
                AccessToken = accessToken
            });
        }

        [HttpPost("refresh")]
        public IActionResult RefreshTokens()
        {
            var refreshToken = Request.Cookies["RefreshToken"];

            if (refreshToken == null)
            {
                return Unauthorized("Please log in to continue.");
            }

            var tokenData = _tokenService.ValidateToken(refreshToken);

            if (tokenData == null)
            {
                return Unauthorized("Session expired, please log in again.");
            }

            var userId = tokenData.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (userId == null || !Guid.TryParse(userId, out Guid userGuid))
            {
                return Unauthorized("Could not parse ID.");
            }

            var user = _dbContext.Users.Find(userGuid);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            string newAccessToken = _tokenService.CreateAccessToken(user);
            string newRefreshToken = _tokenService.CreateRefreshToken(user);

            Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions()
            {
                Expires = DateTimeOffset.Now.AddDays(7),
                HttpOnly = true,
                Secure = true
            });

            return Ok(new
            {
                accessToken = newAccessToken
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Append("RefreshToken", "", new CookieOptions()
            {
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });

            return Ok();
        }
    }
}
