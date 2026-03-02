using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;

namespace ChatApp.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]   
        public int UserId { get; set; }
        public string FirstName { get; set; }= null!;   
        public string LastName { get; set; }    = null!;    
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Conversation> ConversationsAsUser1 { get; set; } = new List<Conversation>();
        public ICollection<Conversation> ConversationsAsUser2 { get; set; } = new List<Conversation>();
    }
}
