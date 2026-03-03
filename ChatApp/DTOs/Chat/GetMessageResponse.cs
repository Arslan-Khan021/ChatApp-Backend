namespace ChatApp.DTOs.Chat
{
    public class GetMessageResponse
    {
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = null!;
        public string MessageText { get; set; } = null!;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }
}
