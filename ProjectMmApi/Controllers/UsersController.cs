using Microsoft.AspNetCore.Mvc;
using ProjectMmApi.Data;
using ProjectMmApi.Interfaces;
using ProjectMmApi.Models.Dtos;
using ProjectMmApi.Models.Entities;
using ProjectMmApi.Utilities;

namespace ProjectMmApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        public UsersController(ApplicationDbContext dbContext,IPasswordHasher passwordHasher, ITokenService tokenService)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        [HttpPost]
        public IActionResult CreateUser(CreateUserDto createUserDto)
        {
            var existingUser = _dbContext.Users.FirstOrDefault(e => e.Email == createUserDto.Email);
            
            if (existingUser != null)
            {
                return BadRequest("User with that email already exists.");
            }

            if (!Validator.IsValidEmail(createUserDto.Email))
            {
                return BadRequest("The email entered is not valid.");
            }

            if (!Validator.IsStrongPassword(createUserDto.Password))
            {
                return BadRequest("Password must be 8+ characters with uppercase, lowercase, digits and no spaces.");
            }

            string passwordHash = _passwordHasher.Hash(createUserDto.Password);

            var newUser = new User()
            {
                Email = createUserDto.Email,
                FullName = createUserDto.FullName,
                Password = passwordHash
            };

            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();

            string accessToken = _tokenService.CreateAccessToken(newUser);
            string refreshToken = _tokenService.CreateRefreshToken(newUser);

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
    }
}
