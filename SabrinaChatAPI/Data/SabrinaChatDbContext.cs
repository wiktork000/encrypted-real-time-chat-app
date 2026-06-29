using ChatApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Data;

public class SabrinaChatDbContext : DbContext
{
    public SabrinaChatDbContext(DbContextOptions<SabrinaChatDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
    public DbSet<Key> Keys { get; set; }
    public DbSet<Message> Messages { get; set; }

    public DbSet<UserCredentials> UsersCredentials { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.PrivateKey)
                .IsRequired();

            entity.Property(e => e.PublicKey)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); 

            // Create unique index on username
            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("IX_Users_Name");
        });

        // Configure UserCredentials entity
        modelBuilder.Entity<UserCredentials>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_UserCredentials_Email");

            entity.HasOne(e => e.User)
                .WithMany(u => u.UsersCredentials)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_UserCredentials_Users_UserId");

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_UserCredentials_UserId");
        });

        // Conversation configuration
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        });

        // ConversationParticipant configuration
        modelBuilder.Entity<ConversationParticipant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.ConversationId }).IsUnique();
            
            entity.HasOne(e => e.User)
                .WithMany(e => e.ConversationParticipants)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Conversation)
                .WithMany(e => e.Participants)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Key configuration
        modelBuilder.Entity<Key>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.KeyValue).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.User)
                      .WithMany(u => u.Keys)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Conversation)
                .WithMany(e => e.Keys)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.FromMessage)
                      .WithMany()
                      .HasForeignKey(e => e.FromMessageId)
                      .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ToMessage)
                      .WithMany()
                      .HasForeignKey(e => e.ToMessageId)
                      .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.UserId, e.ConversationId, e.FromMessageId, e.ToMessageId });

        });

        // Message configuration
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            
            entity.HasOne(e => e.Conversation)
                .WithMany(e => e.Messages)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Author)
                .WithMany(e => e.Messages)
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(tb => tb.HasTrigger("TR_Messages_IncrementUnreadCount"));

        });
    }
}