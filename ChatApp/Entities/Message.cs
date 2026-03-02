using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChatApp.Entities
{
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        [JsonIgnore]
        public Conversation? Conversation { get; set; }
        public int SenderId { get; set; }
        [JsonIgnore]
        public User? Sender { get; set; }
        public string MessageText { get; set; } = null!;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}
