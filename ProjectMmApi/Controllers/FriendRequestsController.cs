using Microsoft.AspNetCore.Mvc;
using ProjectMmApi.Data;
using ProjectMmApi.Models.DTOs;
using ProjectMmApi.Models.Entities;

namespace ProjectMmApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendRequestsController : ControllerBase
    {
        public readonly ApplicationDbContext _dbContext;
        public FriendRequestsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public enum RequestAction
        {
            Accept = 0,
            Reject = 1,
            Cancel = 2
        }

        [HttpPost]
        public IActionResult CreateFriendRequest(HandleFriendRequestDto createFriendRequestDto)
        {
            try
            {
                var senderGuid = GetSenderGuid();
                var receiverGuid = GetReceiverGuid(createFriendRequestDto);

                if (senderGuid == receiverGuid)
                {
                    return BadRequest("Can not send request to self.");
                }

                var existingSentRequest = _dbContext.FriendRequests.FirstOrDefault(fr =>
                    fr.SenderId == senderGuid && fr.ReceiverId == receiverGuid);

                var existingReceivedRequest = _dbContext.FriendRequests.FirstOrDefault(fr =>
                    fr.ReceiverId == senderGuid && fr.SenderId == receiverGuid);

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

                var newFriendRequest = new FriendRequest
                {
                    SenderId = senderGuid,
                    ReceiverId = receiverGuid,
                    Status = RequestStatus.Pending,
                    SentAt = DateTime.UtcNow
                };

                _dbContext.FriendRequests.Add(newFriendRequest);
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
            var senderGuid = GetSenderGuid();
            var receiverGuid = GetReceiverGuid(handleFriendRequestDto);

            string actionString = action.ToString().ToLower(); 

            var request = _dbContext.FriendRequests.FirstOrDefault(fr =>
                fr.SenderId == senderGuid && fr.ReceiverId == receiverGuid);

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
                    _dbContext.FriendRequests.Update(request);
                    break;
                case RequestAction.Reject:
                    request.Status = RequestStatus.Rejected;
                    _dbContext.FriendRequests.Update(request);
                    break;
                case RequestAction.Cancel:
                    _dbContext.FriendRequests.Remove(request);
                    break;
            }

            _dbContext.SaveChanges();

            return Ok($"Successfully {actionString} the request.");
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
