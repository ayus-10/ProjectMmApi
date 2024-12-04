namespace ProjectMmApi.Controllers.Middlewares
{
    public class GuardMiddleware:IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            bool isUserLoggedIn = context.Items.ContainsKey("IsLoggedIn")
                && context.Items["IsLoggedIn"] is bool isLoggedIn
                && isLoggedIn;

            bool isNonProtectedRoute = context.Request.Path.StartsWithSegments("/api/Users")
                || context.Request.Path.StartsWithSegments("/api/Auth");

            if (isUserLoggedIn && isNonProtectedRoute)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("You are already logged in.");
                return;
            }

            if (!isUserLoggedIn && !isNonProtectedRoute)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Please log in to continue.");
                return;
            }

            await next(context);
        }
    }
}
