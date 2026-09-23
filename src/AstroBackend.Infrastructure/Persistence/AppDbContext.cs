using AstroBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AstroBackend.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Profile> Profiles => Set<Profile>();
        public DbSet<NatalChart> NatalCharts => Set<NatalChart>();
        public DbSet<Horoscope> Horoscopes => Set<Horoscope>();
        public DbSet<Astrologer> Astrologers => Set<Astrologer>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
        public DbSet<ForumTopic> ForumTopics => Set<ForumTopic>();
        public DbSet<ForumReply> ForumReplies => Set<ForumReply>();
        public DbSet<Article> Articles => Set<Article>();
        public DbSet<ChatThread> ChatThreads => Set<ChatThread>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).HasMaxLength(255).IsRequired();
                entity.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            });

            // Profile (1 to 1 with User)
            modelBuilder.Entity<Profile>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasOne(p => p.User)
                      .WithOne(u => u.Profile)
                      .HasForeignKey<Profile>(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // NatalChart (1 to 1 with User)
            modelBuilder.Entity<NatalChart>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.HasOne(n => n.User)
                      .WithOne(u => u.NatalChart)
                      .HasForeignKey<NatalChart>(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Horoscope
            modelBuilder.Entity<Horoscope>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.HasIndex(h => new { h.Sign, h.Period, h.PeriodStart }).IsUnique();
                entity.Property(h => h.Sign).HasMaxLength(50).IsRequired();
            });

            // Astrologer
            modelBuilder.Entity<Astrologer>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.DisplayName).HasMaxLength(100).IsRequired();
                entity.Property(a => a.Rating).HasColumnType("decimal(3,1)");

                entity.HasOne(a => a.User)
                      .WithOne(u => u.AstrologerProfile)
                      .HasForeignKey<Astrologer>(a => a.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Booking
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(b => b.Id);

                entity.HasOne(b => b.User)
                      .WithMany(u => u.Bookings)
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(b => b.Astrologer)
                      .WithMany(a => a.Bookings)
                      .HasForeignKey(b => b.AstrologerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // JournalEntry
            modelBuilder.Entity<JournalEntry>(entity =>
            {
                entity.HasKey(j => j.Id);

                entity.HasOne(j => j.User)
                      .WithMany(u => u.JournalEntries)
                      .HasForeignKey(j => j.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ForumTopic
            modelBuilder.Entity<ForumTopic>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Title).HasMaxLength(200).IsRequired();

                entity.HasOne(t => t.User)
                      .WithMany(u => u.ForumTopics)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ForumReply
            modelBuilder.Entity<ForumReply>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.HasOne(r => r.Topic)
                      .WithMany(t => t.Replies)
                      .HasForeignKey(r => r.TopicId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.User)
                      .WithMany(u => u.ForumReplies)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Article
            modelBuilder.Entity<Article>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasIndex(a => a.Slug).IsUnique();
                entity.Property(a => a.Title).HasMaxLength(255).IsRequired();
                entity.Property(a => a.Slug).HasMaxLength(255).IsRequired();

                entity.HasOne(a => a.Author)
                      .WithMany(u => u.Articles)
                      .HasForeignKey(a => a.AuthorId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ChatThread
            modelBuilder.Entity<ChatThread>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Title).HasMaxLength(150).IsRequired();

                entity.HasOne(t => t.User)
                      .WithMany(u => u.ChatThreads)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ChatMessage
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.HasOne(m => m.Thread)
                      .WithMany(t => t.Messages)
                      .HasForeignKey(m => m.ThreadId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.User)
                      .WithMany(u => u.ChatMessages)
                      .HasForeignKey(m => m.UserId)
                      .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }


}
