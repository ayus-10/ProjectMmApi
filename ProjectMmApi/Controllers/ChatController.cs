using Microsoft.AspNetCore.Mvc;
using ProjectMmApi.Data;
using ProjectMmApi.Models.Dtos;
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

                var existingConversation = _dbContext.Conversations
                    .FirstOrDefault(e => e.FriendId == friend.FriendId);

                if (existingConversation != null)
                {
                    return BadRequest("Already started conversation with the receiver.");
                }

                var conversation = new Conversation()
                {
                    FriendId = friend.FriendId
                };

                _dbContext.Conversations.Add(conversation);
                _dbContext.SaveChanges();

                return Ok();
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

        [HttpPost("send")]
        public IActionResult SendMessage(SendMessageDto sendMessageDto)
        {
            try
            {
                var conversationGuid = Helper.ValidateConversationId(sendMessageDto.ConversationId, _dbContext);
                var messageSenderGuid = Helper.GetUserIdFromContext(HttpContext);

                var conversation = _dbContext.Conversations.Find(conversationGuid);
                if (conversation?.ByFriend?.SenderId != messageSenderGuid ||
                    conversation?.ByFriend?.ReceiverId != messageSenderGuid)
                {
                    return BadRequest("You are not allowed chat in this conversation.");
                }

                var newMessage = new Message()
                {
                    ConversationId = conversationGuid,
                    SenderId = messageSenderGuid,
                    MessageText = sendMessageDto.MessageText,
                    IsSeen = false,
                    MessageTime = DateTime.UtcNow
                };

                _dbContext.Messages.Add(newMessage);
                _dbContext.SaveChanges();

                return Ok();
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

        [HttpGet("conversations")]
        public IActionResult GetAllConversations()
        {
            try
            {
                var userGuid = Helper.GetUserIdFromContext(HttpContext);

                var conversations = _dbContext.Conversations
                    .Where(c => c.ByFriend != null &&
                                c.ByFriend.Sender != null &&
                                c.ByFriend.Receiver != null &&
                                (c.ByFriend.SenderId == userGuid || c.ByFriend.ReceiverId == userGuid))
                    .Select(c => new
                    {
                        ReceiverName = c.ByFriend!.SenderId == userGuid
                            ? c.ByFriend.Receiver!.FullName
                            : c.ByFriend.Sender!.FullName,
                        ReceiverEmail = c.ByFriend.SenderId == userGuid
                            ? c.ByFriend.Receiver!.Email
                            : c.ByFriend.Sender!.Email,
                        Message = c.Messages != null
                            ? c.Messages.OrderByDescending(m => m.MessageTime).FirstOrDefault()
                            : null
                    })
                    .ToList();

                return Ok(new { conversations });
            }
            catch (UnauthorizedAccessException u)
            {
                return Unauthorized(u.Message);
            }
        }

        // get all messages for a convo: convoId (get)
        // make message seen: messageId (patch)
    }
}