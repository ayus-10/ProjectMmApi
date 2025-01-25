using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectMmApi.Models.Entities
{
    public class Message
    {
        public Guid MessageId { get; set; }
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public DateTime MessageTime { get; set; }
        public bool IsSeen { get; set; }

        [Column(TypeName = "text")]
        public required string MessageText { get; set; }

        public Conversation? FromConversation { get; set; }
        public User? Sender { get; set; }
    }
}