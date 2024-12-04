using System.IdentityModel.Tokens.Jwt;
using ProjectMmApi.Interfaces;

namespace ProjectMmApi.Controllers.Middlewares
{
    public class AuthMiddleware:IMiddleware
    {
        private readonly ITokenService _tokenService;
        public AuthMiddleware(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var accessTokenString = context.Request.Headers.Authorization.FirstOrDefault();

            var refreshToken = context.Request.Cookies["RefreshToken"] ?? null;

            if (string.IsNullOrEmpty(accessTokenString) || string.IsNullOrEmpty(refreshToken))
            {
                await next(context);
                return;
            }
            else
            {
                var accessTokenStringParts = accessTokenString.Split(" ");
                
                if (accessTokenStringParts.Length != 2
                    || !string.Equals(accessTokenStringParts[0], "Bearer", StringComparison.OrdinalIgnoreCase))
                {
                    await next(context);
                    return;
                }

                var tokenData = _tokenService.ValidateToken(accessTokenStringParts[1]);

                if (tokenData != null)
                {
                    var userId = tokenData.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                    var userEmail = tokenData.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
                    
                    context.Items["UserId"] = userId;
                    context.Items["UserEmail"] = userEmail;
                    context.Items["IsLoggedIn"] = true;
                }
            }

            await next(context);
        }
    }
}
