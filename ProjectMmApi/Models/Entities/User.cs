namespace ProjectMmApi.Models.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }

        public ICollection<Friend> SentRequests { get; set; }
        public ICollection<Friend> ReceivedRequests { get; set; }
    }
}
