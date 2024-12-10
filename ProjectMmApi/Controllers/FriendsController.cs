using Microsoft.AspNetCore.Mvc;
using ProjectMmApi.Data;
using ProjectMmApi.Models.DTOs;
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

        public enum RequestAction
        {
            Accept = 0,
            Reject = 1,
            Cancel = 2
        }

        [HttpGet]
        public IActionResult GetAllFriends()
        {
            try
            {
                Guid senderGuid = GetSenderGuid();

                var friends = _dbContext.Friends
                    .Where(f => (f.SenderId == senderGuid || f.ReceiverId == senderGuid) && f.Status == RequestStatus.Accepted)
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
                Guid senderGuid = GetSenderGuid();

                var sentRequests = _dbContext.Friends
                    .Where(f => f.SenderId == senderGuid && f.Status == RequestStatus.Pending)
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
                   .Where(f => f.ReceiverId == senderGuid && f.Status == RequestStatus.Pending)
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
                Guid senderGuid = GetSenderGuid();

                var sender = _dbContext.Users.Find(senderGuid);
                var userFound = _dbContext.Users.FirstOrDefault(u => u.Email == email);

                if (userFound == null)
                {
                    return NotFound("No user found with that email.");
                }

                var existingSentRequest = _dbContext.Friends
                    .FirstOrDefault(f => f.SenderId == senderGuid && f.ReceiverId == userFound.Id);

                var existingReceivedRequest = _dbContext.Friends
                    .FirstOrDefault(f => f.ReceiverId == senderGuid && f.SenderId == userFound.Id);

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
        public IActionResult CreateFriendRequest(HandleFriendRequestDto createFriendRequestDto)
        {
            try
            {
                Guid senderGuid = GetSenderGuid();
                Guid receiverGuid = GetReceiverGuid(createFriendRequestDto);

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

                return Ok("Successfully sent the friend request.");
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
        public IActionResult AcceptFriendRequest(HandleFriendRequestDto acceptFriendRequestDto)
        {
            try
            {
                return HandleFriendRequest(acceptFriendRequestDto, RequestAction.Accept);
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
        public IActionResult RejectFriendRequest(HandleFriendRequestDto rejectFriendRequestDto)
        {
            try
            {
                return HandleFriendRequest(rejectFriendRequestDto, RequestAction.Reject);
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
        public IActionResult CancelFriendRequest(HandleFriendRequestDto cancelFriendRequestDto)
        {
            try
            {
                return HandleFriendRequest(cancelFriendRequestDto, RequestAction.Cancel);
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

        private IActionResult HandleFriendRequest(HandleFriendRequestDto handleFriendRequestDto, RequestAction action)
        {
            Guid senderGuid = GetSenderGuid();
            Guid receiverGuid = GetReceiverGuid(handleFriendRequestDto);

            string actionString = action.ToString().ToLower();

            var request = _dbContext.Friends
                .FirstOrDefault(f => f.SenderId == senderGuid && f.ReceiverId == receiverGuid);

            if (request == null)
            {
                return BadRequest("Invalid receiver ID.");
            }

            if (request.Status != RequestStatus.Pending)
            {
                return BadRequest($"Can not {actionString} the request now.");
            }

            switch (action)
            {
                case RequestAction.Accept:
                    request.Status = RequestStatus.Accepted;
                    _dbContext.Friends.Update(request);
                    break;
                case RequestAction.Reject:
                    request.Status = RequestStatus.Rejected;
                    _dbContext.Friends.Update(request);
                    break;
                case RequestAction.Cancel:
                    _dbContext.Friends.Remove(request);
                    break;
            }

            _dbContext.SaveChanges();

            return Ok($"Successfully {actionString}ed the request.");
        }

        private Guid GetSenderGuid()
        {
            if (!Guid.TryParse(HttpContext.Items["UserId"]?.ToString(), out Guid senderGuid))
            {
                throw new UnauthorizedAccessException("Invalid sender ID.");
            }

            return senderGuid;
        }

        private Guid GetReceiverGuid(HandleFriendRequestDto handleFriendRequestDto)
        {
            if (!Guid.TryParse(handleFriendRequestDto.ReceiverId, out Guid receiverGuid)
                || _dbContext.Users.Find(receiverGuid) == null)
            {
                throw new BadHttpRequestException("Invalid receiver ID.");
            }

            return receiverGuid;
        }
    }
}
