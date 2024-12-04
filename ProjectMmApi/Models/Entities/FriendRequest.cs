namespace ProjectMmApi.Models.Entities
{
    public enum RequestStatus
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2,
    }

    public class FriendRequest
    {
        public Guid FriendRequestId { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public RequestStatus Status { get; set; }
    }
}
