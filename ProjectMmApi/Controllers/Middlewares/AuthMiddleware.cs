namespace ProjectMmApi.Controllers.Middlewares
{
    public class AuthMiddleware:IMiddleware
    {
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
            }

            Console.WriteLine(tokenString);

            await next(context);
        }
    }
}
