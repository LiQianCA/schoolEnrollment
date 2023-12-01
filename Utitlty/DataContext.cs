using Microsoft.EntityFrameworkCore;
using schoolEnrollment.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace schoolEnrollment.Utitlty
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
            modelBuilder.Entity<Course>();
            modelBuilder.Entity<SelectList>();
            modelBuilder.Entity<Enrollment>();

            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<Course> Course { get; set; }
        public virtual DbSet<SelectList> SelectList { get; set; }
        public virtual DbSet<Enrollment> Enrollment { get; set; }
    }
}
