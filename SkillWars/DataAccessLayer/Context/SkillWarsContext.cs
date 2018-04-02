using Common.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Context
{
    public class SkillWarsContext : DbContext
    {
        public SkillWarsContext(DbContextOptions<SkillWarsContext> options) : base(options) { }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<TokenEntity> Tokens { get; set; }       

        public DbSet<LobbieEntity> Lobbies { get; set; }
        public DbSet<TeamEntity> Teams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TeamEntity>().HasMany(t => t.Users)
                .WithOne(u => u.Team)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
