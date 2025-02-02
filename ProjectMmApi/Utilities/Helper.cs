using ProjectMmApi.Data;
using ProjectMmApi.Models.Entities;

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

        public static Guid ValidateConversationId(string id, ApplicationDbContext dbContext, Guid userGuid)
        {
            if (!Guid.TryParse(id, out Guid conversationGuid))
            {
                throw new BadHttpRequestException("Invalid conversation ID.");
            }

            var conversation = dbContext.Conversations
                .FirstOrDefault(c => c.ConversationId == conversationGuid &&
                                     c.ByFriend != null &&
                                     (c.ByFriend.SenderId == userGuid || c.ByFriend.ReceiverId == userGuid))
                ?? throw new BadHttpRequestException("You are not allowed to chat in this conversation.");

            return conversationGuid;
        }

        public static Message GetValidMessage(string id, Guid conversationGuid, ApplicationDbContext dbContext)
        {
            if (!Guid.TryParse(id, out Guid messageGuid))
            {
                throw new BadHttpRequestException("Invalid message ID.");
            }

            var message = dbContext.Messages
                .FirstOrDefault(m => m.ConversationId == conversationGuid)
                ?? throw new BadHttpRequestException("You are not allowed to interact with this message.");

            return message;
        }
    }
}