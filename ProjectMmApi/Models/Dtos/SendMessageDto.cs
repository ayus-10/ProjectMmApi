namespace ProjectMmApi.Models.Dtos
{
    public class SendMessageDto
    {
        public required string ConversationId { get; set; }
        public required string MessageText { get; set; }
    }
}