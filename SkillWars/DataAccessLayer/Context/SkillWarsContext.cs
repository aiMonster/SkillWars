using Common.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Context
{
    public class SkillWarsContext : DbContext
    {
        public SkillWarsContext(DbContextOptions<SkillWarsContext> options) : base(options) { }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<NotificationEntity> Notifications { get; set; }
        public DbSet<SuggestionEntity> Suggestions { get; set; }
        public DbSet<TokenEntity> Tokens { get; set; }       

        public DbSet<LobbieEntity> Lobbies { get; set; }
        public DbSet<TeamEntity> Teams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TeamEntity>().HasMany(t => t.Users)
                .WithOne(u => u.Team)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserNotificationsEntity>()
            .HasKey(t => new { t.UserId, t.NotificationId });

            modelBuilder.Entity<UserNotificationsEntity>()
                .HasOne(sc => sc.User)
                .WithMany(s => s.Notifications)
                .HasForeignKey(sc => sc.UserId);

            modelBuilder.Entity<UserNotificationsEntity>()
                .HasOne(sc => sc.Notification)
                .WithMany(c => c.Users)
                .HasForeignKey(sc => sc.NotificationId);
        }
    }
}
