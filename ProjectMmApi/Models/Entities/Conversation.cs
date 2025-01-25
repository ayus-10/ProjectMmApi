namespace ProjectMmApi.Models.Entities
{
    public class Conversation
    {
        public Guid ConversationId { get; set; }
        public Guid FriendId { get; set; }
        public Guid LastMessageId { get; set; }
        public DateTime LastMessageTime { get; set; }
        public bool IsSeen { get; set; }

        public Friend ByFriend { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}