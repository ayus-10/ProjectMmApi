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
            var tokenStringValues = context.Request.Headers.Authorization;
            var tokenString = tokenStringValues.FirstOrDefault();

            if (string.IsNullOrEmpty(tokenString))
            {
                await next(context);
                return;
            }
            else
            {
                var tokenStringParts = tokenString.Split(" ");
                
                if (tokenStringParts.Length != 2
                    || !string.Equals(tokenStringParts[0], "Bearer", StringComparison.OrdinalIgnoreCase))
                {
                    await next(context);
                    return;
                }

                var tokenData = _tokenService.ValidateToken(tokenStringParts[1]);

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
