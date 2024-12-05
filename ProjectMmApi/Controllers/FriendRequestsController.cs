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

        // TODO: remove all redundant code, add try catch if needed

        [HttpPost]
        public IActionResult CreateFriendRequest(HandleFriendRequestDto createFriendRequestDto)
        {
            if (!Guid.TryParse(HttpContext.Items["UserId"]?.ToString(), out Guid senderGuid))
            {
                return Unauthorized("Invalid sender ID");
            }

            if (!Guid.TryParse(createFriendRequestDto.ReceiverId, out Guid receiverGuid)
                || _dbContext.Users.Find(receiverGuid) == null)
            {
                return BadRequest("Invalid receiver ID");
            }

            if (senderGuid == receiverGuid)
            {
                return BadRequest("Can not send request to self");
            }

            var existingSentRequest = _dbContext.FriendRequests.FirstOrDefault(fr =>
                fr.SenderId == senderGuid && fr.ReceiverId == receiverGuid);

            var existingReceivedRequest = _dbContext.FriendRequests.FirstOrDefault(fr => 
                fr.ReceiverId == senderGuid && fr.SenderId == receiverGuid);

            if (existingSentRequest != null)
            {
                return BadRequest("Duplicate requests are not allowed");
            }

            if (existingReceivedRequest != null)
            {
                if (existingReceivedRequest.Status == RequestStatus.Pending)
                {
                    return BadRequest("Already received request from the same user");
                }
                else if (existingReceivedRequest.Status == RequestStatus.Accepted)
                {
                    return BadRequest("Already friends with the user");
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

            return Ok("Successfully sent the friend request");
        }

        [HttpPatch("accept")]
        public IActionResult AcceptFriendRequest(HandleFriendRequestDto acceptFriendRequestDto)
        {
            if (!Guid.TryParse(HttpContext.Items["UserId"]?.ToString(), out Guid senderGuid))
            {
                return Unauthorized("Invalid sender ID");
            }

            if (!Guid.TryParse(acceptFriendRequestDto.ReceiverId, out Guid receiverGuid)
                || _dbContext.Users.Find(receiverGuid) == null)
            {
                return BadRequest("Invalid receiver ID");
            }

            var request = _dbContext.FriendRequests.FirstOrDefault(fr => fr.SenderId == senderGuid && fr.ReceiverId == receiverGuid);

            if (request == null)
            {
                return BadRequest("Invalid receiver ID");
            }

            if (request.Status != RequestStatus.Pending)
            {
                return BadRequest("Can not accept the request now");
            }

            request.Status = RequestStatus.Accepted;

            _dbContext.FriendRequests.Update(request);
            _dbContext.SaveChanges();

            return Ok("Successfully accepted the request");
        }


        [HttpPatch("reject")]
        public IActionResult RejectFriendRequest(HandleFriendRequestDto rejectFriendRequestDto)
        {
            if (!Guid.TryParse(HttpContext.Items["UserId"]?.ToString(), out Guid senderGuid))
            {
                return Unauthorized("Invalid sender ID");
            }

            if (!Guid.TryParse(rejectFriendRequestDto.ReceiverId, out Guid receiverGuid)
                || _dbContext.Users.Find(receiverGuid) == null)
            {
                return BadRequest("Invalid receiver ID");
            }

            var request = _dbContext.FriendRequests.FirstOrDefault(fr => fr.SenderId == senderGuid && fr.ReceiverId == receiverGuid);

            if (request == null)
            {
                return BadRequest("Invalid receiver ID");
            }

            if (request.Status != RequestStatus.Pending)
            {
                return BadRequest("Can not reject the request now");
            }

            request.Status = RequestStatus.Rejected;

            _dbContext.FriendRequests.Update(request);
            _dbContext.SaveChanges();

            return Ok("Successfully rejected the request");
        }

        [HttpDelete]
        public IActionResult CancelFriendRequest(HandleFriendRequestDto cancelFriendRequestDto)
        {
            if (!Guid.TryParse(HttpContext.Items["UserId"]?.ToString(), out Guid senderGuid))
            {
                return Unauthorized("Invalid sender ID");
            }

            if (!Guid.TryParse(cancelFriendRequestDto.ReceiverId, out Guid receiverGuid)
                || _dbContext.Users.Find(receiverGuid) == null)
            {
                return BadRequest("Invalid receiver ID");
            }

            var request = _dbContext.FriendRequests.FirstOrDefault(fr => fr.SenderId == senderGuid && fr.ReceiverId == receiverGuid);

            if (request == null)
            {
                return BadRequest("Invalid receiver ID");
            }

            if (request.Status != RequestStatus.Pending)
            {
                return BadRequest("Can not cancel the request now");
            }

            _dbContext.FriendRequests.Remove(request);
            _dbContext.SaveChanges();

            return Ok("Successfully canceled the request");
        }
    }
}
