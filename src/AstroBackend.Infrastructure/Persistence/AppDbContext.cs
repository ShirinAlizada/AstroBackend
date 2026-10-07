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
        public DbSet<ShopProduct> ShopProducts => Set<ShopProduct>();
        public DbSet<ShopOrder> ShopOrders => Set<ShopOrder>();
        public DbSet<ShopOrderItem> ShopOrderItems => Set<ShopOrderItem>();
        public DbSet<ShopProductReview> ShopProductReviews => Set<ShopProductReview>();
        public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
        public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
        public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();
        public DbSet<AiUsageLog> AiUsageLogs => Set<AiUsageLog>();

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
                      .OnDelete(DeleteBehavior.Restrict);

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
                entity.Property(t => t.TitleEn).HasMaxLength(200);
                entity.Property(t => t.TitleRu).HasMaxLength(200);

                entity.HasOne(t => t.User)
                      .WithMany(u => u.ForumTopics)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
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
                entity.Property(a => a.TitleEn).HasMaxLength(255);
                entity.Property(a => a.TitleRu).HasMaxLength(255);
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
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ShopProduct
            modelBuilder.Entity<ShopProduct>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.Slug).IsUnique();
                entity.Property(p => p.Slug).HasMaxLength(120).IsRequired();
                entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
                entity.Property(p => p.NameEn).HasMaxLength(200);
                entity.Property(p => p.NameRu).HasMaxLength(200);
                entity.Property(p => p.Description).HasMaxLength(2000).IsRequired();
                entity.Property(p => p.DescriptionEn).HasMaxLength(2000);
                entity.Property(p => p.DescriptionRu).HasMaxLength(2000);
                entity.Property(p => p.UnitLabel).HasMaxLength(50);
                entity.Property(p => p.UnitLabelEn).HasMaxLength(50);
                entity.Property(p => p.UnitLabelRu).HasMaxLength(50);
                entity.Property(p => p.ImageUrl).HasMaxLength(500);
            });

            // ShopOrder
            modelBuilder.Entity<ShopOrder>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.FullName).HasMaxLength(150).IsRequired();
                entity.Property(o => o.Phone).HasMaxLength(50).IsRequired();
                entity.Property(o => o.Address).HasMaxLength(500).IsRequired();

                entity.HasOne(o => o.User)
                      .WithMany(u => u.ShopOrders)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ShopOrderItem
            modelBuilder.Entity<ShopOrderItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.ProductName).HasMaxLength(200).IsRequired();

                entity.HasOne(i => i.Order)
                      .WithMany(o => o.Items)
                      .HasForeignKey(i => i.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.Product)
                      .WithMany(p => p.OrderItems)
                      .HasForeignKey(i => i.ProductId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ShopProductReview
            modelBuilder.Entity<ShopProductReview>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasIndex(r => new { r.ProductId, r.UserId }).IsUnique();
                entity.Property(r => r.Rating).IsRequired();
                entity.Property(r => r.Comment).HasMaxLength(2000);

                entity.HasOne(r => r.Product)
                      .WithMany(p => p.Reviews)
                      .HasForeignKey(r => r.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.User)
                      .WithMany(u => u.ShopProductReviews)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // WishlistItem
            modelBuilder.Entity<WishlistItem>(entity =>
            {
                entity.HasKey(w => w.Id);
                entity.HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();

                entity.HasOne(w => w.User)
                      .WithMany(u => u.WishlistItems)
                      .HasForeignKey(w => w.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(w => w.Product)
                      .WithMany()
                      .HasForeignKey(w => w.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Type).HasMaxLength(50).IsRequired();
                entity.Property(n => n.Title).HasMaxLength(200).IsRequired();
                entity.Property(n => n.Body).HasMaxLength(500);
                entity.Property(n => n.Link).HasMaxLength(300);
                entity.HasIndex(n => new { n.UserId, n.CreatedAt });

                entity.HasOne(n => n.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // SubscriptionPlan
            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasAlternateKey(p => p.Key);
                entity.Property(p => p.Key).HasMaxLength(50).IsRequired();
                entity.Property(p => p.Name).HasMaxLength(100).IsRequired();
                entity.Property(p => p.Tagline).HasMaxLength(300);
                entity.Property(p => p.TaglineEn).HasMaxLength(300);
                entity.Property(p => p.TaglineRu).HasMaxLength(300);
                entity.Property(p => p.BillingPeriod).HasMaxLength(20);

                // Features: List<string> <-> JSON mətn sütunu (SQL Server-də native massiv dəstəyi yoxdur).
                entity.Property(p => p.Features)
                      .HasConversion(
                          v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                          v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                      .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                          (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
                          v => v.Aggregate(0, (hash, s) => HashCode.Combine(hash, s.GetHashCode())),
                          v => v.ToList()));

                // FeaturesEn/FeaturesRu: eyni naxış, amma NULL-a icazə verilir (tərcümə hələ
                // doldurulmayan paketlər üçün — göstərmə qatı belə halda AZ-a geri qayıdır).
                entity.Property(p => p.FeaturesEn)
                      .HasConversion(
                          v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                          v => v == null ? null : System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null))
                      .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>?>(
                          (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
                          v => (v ?? new()).Aggregate(0, (hash, s) => HashCode.Combine(hash, s.GetHashCode())),
                          v => v == null ? null : v.ToList()));
                entity.Property(p => p.FeaturesRu)
                      .HasConversion(
                          v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                          v => v == null ? null : System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null))
                      .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>?>(
                          (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
                          v => (v ?? new()).Aggregate(0, (hash, s) => HashCode.Combine(hash, s.GetHashCode())),
                          v => v == null ? null : v.ToList()));
            });

            // UserSubscription (1 to 1 with User)
            modelBuilder.Entity<UserSubscription>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasIndex(s => s.UserId).IsUnique();
                entity.Property(s => s.PlanKey).HasMaxLength(50).IsRequired();
                entity.Property(s => s.Status).HasMaxLength(20).IsRequired();
                entity.Property(s => s.BillingPeriod).HasMaxLength(20).IsRequired();

                entity.HasOne(s => s.User)
                      .WithOne(u => u.Subscription)
                      .HasForeignKey<UserSubscription>(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Plan)
                      .WithMany()
                      .HasForeignKey(s => s.PlanKey)
                      .HasPrincipalKey(p => p.Key)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // PaymentTransaction (append-only jurnal)
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.PlanKey).HasMaxLength(50).IsRequired();
                entity.Property(t => t.Provider).HasMaxLength(50).IsRequired();
                entity.Property(t => t.Status).HasMaxLength(20).IsRequired();
                entity.Property(t => t.Note).HasMaxLength(500);
                entity.Property(t => t.BillingPeriod).HasMaxLength(20).IsRequired();
                entity.HasIndex(t => new { t.UserId, t.CreatedAt });

                entity.HasOne(t => t.User)
                      .WithMany(u => u.Payments)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Plan)
                      .WithMany()
                      .HasForeignKey(t => t.PlanKey)
                      .HasPrincipalKey(p => p.Key)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ContactMessage ("Əlaqə" formu)
            modelBuilder.Entity<ContactMessage>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Name).HasMaxLength(150).IsRequired();
                entity.Property(m => m.Email).HasMaxLength(255).IsRequired();
                entity.Property(m => m.Message).HasMaxLength(4000).IsRequired();
                entity.HasIndex(m => m.CreatedAt);
            });

            // PushSubscription (Web Push / VAPID)
            modelBuilder.Entity<PushSubscription>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.Endpoint).IsUnique();
                entity.Property(p => p.Endpoint).HasMaxLength(500).IsRequired();
                entity.Property(p => p.P256dh).HasMaxLength(300).IsRequired();
                entity.Property(p => p.AuthKey).HasMaxLength(100).IsRequired();

                entity.HasOne(p => p.User)
                      .WithMany(u => u.PushSubscriptions)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // AiUsageLog (AIController-in gündəlik AI istifadə sayğacı)
            modelBuilder.Entity<AiUsageLog>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.HasIndex(l => new { l.UserId, l.UsageDate }).IsUnique();

                entity.HasOne(l => l.User)
                      .WithMany()
                      .HasForeignKey(l => l.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }




}
