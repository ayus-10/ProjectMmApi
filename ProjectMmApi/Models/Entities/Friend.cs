namespace ProjectMmApi.Models.Entities
{
    public enum RequestStatus
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2
    }

    public class Friend
    {
        public Guid FriendId { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public RequestStatus Status { get; set; }
        public DateTime SentAt { get; set; }

        public User Sender { get; set; }
        public User Receiver { get; set; }
    }
}
