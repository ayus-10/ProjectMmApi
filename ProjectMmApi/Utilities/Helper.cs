using ProjectMmApi.Data;

namespace ProjectMmApi.Utilities
{
    public static class Helper
    {
        public static Guid GetUserIdFromContext(HttpContext httpContext)
        {
            if (!Guid.TryParse(httpContext.Items["UserId"]?.ToString(), out Guid userGuid))
            {
                throw new UnauthorizedAccessException("Please log in.");
            }

            return userGuid;
        }

        public static Guid ValidateUserId(string id, ApplicationDbContext dbContext)
        {
            if (!Guid.TryParse(id, out Guid userGuid)
                || dbContext.Users.Find(userGuid) == null)
            {
                throw new BadHttpRequestException("Invalid user ID.");
            }

            return userGuid;
        }
    }
}