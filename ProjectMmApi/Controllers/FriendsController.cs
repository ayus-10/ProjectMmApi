using System;
using Microsoft.AspNetCore.Mvc;
using ProjectMmApi.Data;
using ProjectMmApi.Models.Entities;

namespace ProjectMmApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendsController : ControllerBase
    {
        public readonly ApplicationDbContext _dbContext;
        public FriendsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetAllFriends()
        {
            try
            {
                Guid userGuid = GetUserIdFromContext();

                var friends = _dbContext.Friends
                    .Where(f => (f.SenderId == userGuid || f.ReceiverId == userGuid) && f.Status == RequestStatus.Accepted)
                    .Select(f => new
                    {
                        SenderEmail = f.Sender.Email,
                        SenderFullName = f.Sender.FullName,
                        ReceiverEmail = f.Receiver.Email,
                        ReceiverFullName = f.Receiver.FullName,
                        f.SenderId,
                        f.ReceiverId,
                        f.Status,
                        f.SentAt,
                        f.FriendId
                    })
                    .ToList();

                return Ok(new { friends });

            }
            catch (UnauthorizedAccessException u)
            {
                return Unauthorized(u.Message);
            }
        }

        [HttpGet("requests")]
        public IActionResult GetAllFriendRequests()
        {
            try
            {
                Guid userGuid = GetUserIdFromContext();

                var sentRequests = _dbContext.Friends
                    .Where(f => f.SenderId == userGuid && f.Status == RequestStatus.Pending)
                    .Select(f => new
                    {
                        SenderEmail = f.Sender.Email,
                        SenderFullName = f.Sender.FullName,
                        ReceiverEmail = f.Receiver.Email,
                        ReceiverFullName = f.Receiver.FullName,
                        f.SenderId,
                        f.ReceiverId,
                        f.Status,
                        f.SentAt,
                        f.FriendId
                    })
                    .ToList();

                var receivedRequests = _dbContext.Friends
                   .Where(f => f.ReceiverId == userGuid && f.Status == RequestStatus.Pending)
                   .Select(f => new
                   {
                       SenderEmail = f.Sender.Email,
                       SenderFullName = f.Sender.FullName,
                       ReceiverEmail = f.Receiver.Email,
                       ReceiverFullName = f.Receiver.FullName,
                       f.SenderId,
                       f.ReceiverId,
                       f.Status,
                       f.SentAt,
                       f.FriendId
                   })
                   .ToList();

                return Ok(new
                {
                    sent = sentRequests,
                    received = receivedRequests
                });
            }
            catch (UnauthorizedAccessException u)
            {
                return Unauthorized(u.Message);
            }
        }

        [HttpGet("find")]
        public IActionResult FindFriend([FromQuery] string email)
        {
            try
            {
                Guid userGuid = GetUserIdFromContext();

                var userFound = _dbContext.Users.FirstOrDefault(u => u.Email == email);

                if (userFound == null)
                {
                    return NotFound("No user found with that email.");
                }

                var existingSentRequest = _dbContext.Friends
                    .FirstOrDefault(f => f.SenderId == userGuid && f.ReceiverId == userFound.Id);

                var existingReceivedRequest = _dbContext.Friends
                    .FirstOrDefault(f => f.ReceiverId == userGuid && f.SenderId == userFound.Id);

                var errorMessage = "";

                if (existingReceivedRequest != null)
                {
                    if (existingReceivedRequest.Status == RequestStatus.Accepted)
                    {
                        errorMessage = "Already friends with the user.";
                    }
                    else if (existingReceivedRequest.Status == RequestStatus.Pending)
                    {
                        errorMessage = "Already received request from the same user.";
                    }
                }

                if (existingSentRequest != null)
                {
                    if (existingSentRequest.Status == RequestStatus.Accepted)
                    {
                        errorMessage = "Already friends with the user.";
                    }
                    else if (existingSentRequest.Status == RequestStatus.Pending)
                    {
                        errorMessage = "Already sent request to the same user.";
                    }
                }

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    return BadRequest(errorMessage);
                }

                return Ok(new
                {
                    userFound.Id,
                    userFound.Email,
                    userFound.FullName
                });

            }
            catch (UnauthorizedAccessException u)
            {
                return Unauthorized(u.Message);
            }
        }

        [HttpPost]
        public IActionResult CreateFriendRequest([FromQuery] string receiverId)
        {
            try
            {
                Guid senderGuid = GetUserIdFromContext();
                Guid receiverGuid = ValidateUserId(receiverId);

                if (senderGuid == receiverGuid)
                {
                    return BadRequest("Can not send request to self.");
                }

                var existingSentRequest = _dbContext.Friends
                    .FirstOrDefault(f => f.SenderId == senderGuid && f.ReceiverId == receiverGuid);

                var existingReceivedRequest = _dbContext.Friends
                    .FirstOrDefault(f => f.ReceiverId == senderGuid && f.SenderId == receiverGuid);

                if (existingSentRequest != null)
                {
                    return BadRequest("Duplicate requests are not allowed.");
                }

                if (existingReceivedRequest != null)
                {
                    if (existingReceivedRequest.Status == RequestStatus.Pending)
                    {
                        return BadRequest("Already received request from the same user.");
                    }
                    else if (existingReceivedRequest.Status == RequestStatus.Accepted)
                    {
                        return BadRequest("Already friends with the user.");
                    }
                }

                var newFriendRequest = new Friend
                {
                    SenderId = senderGuid,
                    ReceiverId = receiverGuid,
                    Status = RequestStatus.Pending,
                    SentAt = DateTime.UtcNow
                };

                _dbContext.Friends.Add(newFriendRequest);
                _dbContext.SaveChanges();

                var sentRequest = _dbContext.Friends
                    .Where(f => f.FriendId == newFriendRequest.FriendId)
                    .Select(f => new
                    {
                        SenderEmail = f.Sender.Email,
                        SenderFullName = f.Sender.FullName,
                        ReceiverEmail = f.Receiver.Email,
                        ReceiverFullName = f.Receiver.FullName,
                        f.SenderId,
                        f.ReceiverId,
                        f.Status,
                        f.SentAt,
                        f.FriendId
                    })
                    .FirstOrDefault();

                return Ok(new
                {
                    request = sentRequest
                });
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

        [HttpPatch("accept")]
        public IActionResult AcceptFriendRequest([FromQuery] string senderId)
        {
            try
            {
                Guid receiverGuid = GetUserIdFromContext();
                Guid senderGuid = ValidateUserId(senderId);

                var request = GetPendingFriendRequest(senderGuid, receiverGuid);

                request.Status = RequestStatus.Accepted;
                _dbContext.Friends.Update(request);
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


        [HttpPatch("reject")]
        public IActionResult RejectFriendRequest([FromQuery] string senderId)
        {
            try
            {
                Guid receiverGuid = GetUserIdFromContext();
                Guid senderGuid = ValidateUserId(senderId);

                var request = GetPendingFriendRequest(senderGuid, receiverGuid);

                request.Status = RequestStatus.Rejected;
                _dbContext.Friends.Update(request);
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

        [HttpDelete]
        public IActionResult CancelFriendRequest([FromQuery] string receiverId)
        {
            try
            {
                Guid senderGuid = GetUserIdFromContext();
                Guid receiverGuid = ValidateUserId(receiverId);

                var request = GetPendingFriendRequest(senderGuid, receiverGuid);

                _dbContext.Friends.Remove(request);
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

        private Guid GetUserIdFromContext()
        {
            if (!Guid.TryParse(HttpContext.Items["UserId"]?.ToString(), out Guid userGuid))
            {
                throw new UnauthorizedAccessException("Please log in.");
            }

            return userGuid;
        }

        private Guid ValidateUserId(string id)
        {
            if (!Guid.TryParse(id, out Guid userGuid)
                || _dbContext.Users.Find(userGuid) == null)
            {
                throw new BadHttpRequestException("Invalid user ID.");
            }

            return userGuid;
        }

        private Friend GetPendingFriendRequest(Guid sender, Guid receiver)
        {
            var request = _dbContext.Friends
                    .FirstOrDefault(f => f.SenderId == sender && f.ReceiverId == receiver)
                    ?? throw new BadHttpRequestException("Invalid user ID.");

            if (request.Status != RequestStatus.Pending)
            {
                throw new BadHttpRequestException("Can not process the request now.");
            }

            return request;
        }
    }
}
