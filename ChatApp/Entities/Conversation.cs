using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChatApp.Entities
{
    public class Conversation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConversationId { get; set; }
        [JsonIgnore]
        public User? User1 { get; set; }
        public int User1Id { get; set; }
        [JsonIgnore]
        public User? User2 { get; set; }
        public int User2Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Message> messages { get; set; } = new List<Message>();
    }
}
