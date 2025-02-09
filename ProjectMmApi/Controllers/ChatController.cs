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
                var messageSenderGuid = Helper.GetUserIdFromContext(HttpContext);
                var conversationGuid = Helper.ValidateConversationId(
                    sendMessageDto.ConversationId,
                    _dbContext,
                    messageSenderGuid);

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
                        Receiver = c.ByFriend!.SenderId == userGuid
                            ? c.ByFriend.Receiver
                            : c.ByFriend.Sender,
                        Message = c.Messages != null
                            ? c.Messages.OrderByDescending(m => m.MessageTime).FirstOrDefault()
                            : null
                    })
                    .Select(x => new
                    {
                        ReceiverName = x.Receiver!.FullName,
                        ReceiverEmail = x.Receiver!.Email,
                        Message = new
                        {
                            x.Message!.MessageTime,
                            x.Message!.IsSeen,
                            LastMessage = x.Message!.MessageText,
                            SentBy = x.Message!.Sender!.Email
                        }
                    })
                    .ToList();

                return Ok(new { conversations });
            }
            catch (UnauthorizedAccessException u)
            {
                return Unauthorized(u.Message);
            }
        }

        [HttpGet("messages")]
        public IActionResult GetConversationMessages([FromQuery] string conversationId)
        {
            try
            {
                var userGuid = Helper.GetUserIdFromContext(HttpContext);
                var conversationGuid = Helper.ValidateConversationId(
                    conversationId,
                    _dbContext,
                    userGuid);

                var messages = _dbContext.Conversations
                    .Where(c => c.ConversationId == conversationGuid)
                    .Select(c => c.Messages)
                    .ToList();

                return Ok(new { messages });
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

        [HttpPatch("seen")]
        public IActionResult MarkMessageSeen([FromQuery] string messageId, [FromQuery] string conversationId)
        {
            try
            {
                var userGuid = Helper.GetUserIdFromContext(HttpContext);
                var conversationGuid = Helper.ValidateConversationId(
                    conversationId,
                    _dbContext,
                    userGuid);

                var message = Helper.GetValidMessage(messageId, conversationGuid, _dbContext);
                message.IsSeen = true;

                _dbContext.Messages.Update(message);
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
    }
}