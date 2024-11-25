using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ProjectMmApi.Interfaces;
using ProjectMmApi.Models.Entities;

namespace ProjectMmApi.Services
{
    public class TokenService:ITokenService
    {
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        public TokenService(IConfiguration config)
        {
            _jwtKey = config["JwtConfig:Key"] ?? throw new ArgumentNullException(nameof(config), "JWT key is null");
            _jwtIssuer= config["JwtConfig:Issuer"] ?? throw new ArgumentNullException(nameof(config), "JWT Issuer is null");
            _jwtAudience = config["JwtConfig:Audience"] ?? throw new ArgumentNullException(nameof(config), "JWT Audience is null");
        }

        public string CreateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };
            return CreateToken(claims, TimeSpan.FromMinutes(15));
        }

        public string CreateRefreshToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
            };
            return CreateToken(claims, TimeSpan.FromDays(7));
        }

        public string CreateToken(Claim[] claims, TimeSpan expiry)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.Add(expiry),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var validationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = _jwtIssuer,
                    ValidAudience = _jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };

                return tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch
            {
                return null;
            }
        }
    }
}
