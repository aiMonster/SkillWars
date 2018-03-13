using DataAccessLayer.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Context
{
    public class SkillWarsContext : DbContext
    {
        public SkillWarsContext(DbContextOptions<SkillWarsContext> options) : base(options) { }

        public DbSet<UserEntity> Users { get; set; }
    }
}
