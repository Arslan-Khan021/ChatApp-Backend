namespace ChatApp.DTOs.Chat
{
    public class ChatUserDto
    {
        public int UserId { get; set; }

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? LastMessage { get; set; }

        public DateTime? LastMessageTime { get; set; }
        public int? ConversationId { get; set; } = null;

        public int UnreadCount { get; set; }
    }
}
