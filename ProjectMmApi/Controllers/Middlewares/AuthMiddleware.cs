using ProjectMmApi.Interfaces;

namespace ProjectMmApi.Controllers.Middlewares
{
    public class AuthMiddleware:IMiddleware
    {
        private ITokenService _tokenService;
        public AuthMiddleware(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var tokenStringValues = context.Request.Headers.Authorization;
            var tokenString = tokenStringValues.FirstOrDefault();

            if (String.IsNullOrEmpty(tokenString) || tokenString.Split(" ").Length != 2)
            {
                await next(context);
                return;
            }

            string token = tokenString.Split(" ")[1];

            var tokenData = _tokenService.ValidateToken(token);

            await next(context);
        }
    }
}