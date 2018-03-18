using Common.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Context
{
    public class SkillWarsContext : DbContext
    {
        public SkillWarsContext(DbContextOptions<SkillWarsContext> options) : base(options) { }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<TokenEntity> Tokens { get; set; }       
    }
}
