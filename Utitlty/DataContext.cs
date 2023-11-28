using Microsoft.EntityFrameworkCore;
using OnlineCourse.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace OnlineCourse.Utitlty
{
    public class DataContext : DbContext
    {
        public DataContext()
        {

        }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConfigurationHelper.Instance.GetConnectionString("DefaultConnection", ""));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>();
            modelBuilder.Entity<Role>();
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Role> Role { get; set; }
    }
}
