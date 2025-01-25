namespace ProjectMmApi.Models.Entities
{
    public class Conversation
    {
        public Guid ConversationId { get; set; }
        public Guid FriendId { get; set; }

        public Friend? ByFriend { get; set; }
        public ICollection<Message>? Messages { get; set; }
    }
}