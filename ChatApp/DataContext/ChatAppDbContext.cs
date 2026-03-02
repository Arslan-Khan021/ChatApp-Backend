using ChatApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.DataContext
{
    public class ChatAppDbContext:DbContext
    {
        public ChatAppDbContext(DbContextOptions<ChatAppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }  
        public DbSet<Conversation> Conversations { get; set; }  
        public DbSet<Message> Messages { get; set; }    
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Conversation>()
           .HasOne(c => c.User1)
           .WithMany(u => u.ConversationsAsUser1)
           .HasForeignKey(c => c.User1Id)
           .OnDelete(DeleteBehavior.SetNull); 

            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.User2)
                .WithMany(u => u.ConversationsAsUser2)
                .HasForeignKey(c => c.User2Id)
                .OnDelete(DeleteBehavior.SetNull);


            modelBuilder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
