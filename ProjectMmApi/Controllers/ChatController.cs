using Microsoft.AspNetCore.Mvc;
using ProjectMmApi.Data;
using ProjectMmApi.Models.Entities;
using ProjectMmApi.Utilities;

namespace ProjectMmApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        public ChatController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("start")]
        public IActionResult StartConversation([FromQuery] string messageReceiverId)
        {
            try
            {
                var messageSenderGuid = Helper.GetUserIdFromContext(HttpContext);
                var messageReceiverGuid = Helper.ValidateUserId(messageReceiverId, _dbContext);

                var friend = _dbContext.Friends
                    .FirstOrDefault(e =>
                        (e.SenderId == messageSenderGuid && e.ReceiverId == messageReceiverGuid) ||
                        (e.SenderId == messageReceiverGuid && e.ReceiverId == messageSenderGuid));

                if (friend == null)
                {
                    return BadRequest("You are not friends with the receiver.");
                }

                var conversation = new Conversation()
                {
                    FriendId = friend.FriendId
                };

                _dbContext.Conversations.Add(conversation);
                _dbContext.SaveChanges();

                return Ok("Conversation started successfully.");
            }
            catch (UnauthorizedAccessException u)
            {
                return Unauthorized(u.Message);
            }
            catch (BadHttpRequestException b)
            {
                return BadRequest(b.Message);
            }
        }
    }
}