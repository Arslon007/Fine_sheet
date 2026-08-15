using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<Bonus> Bonus { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<Fine> Fine { get; set; }
        public DbSet<Backup> Backup { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Position).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Fine>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Reason).IsRequired().HasMaxLength(500);
                entity.Property(f => f.Amount).HasColumnType("decimal(18,2)");

                entity.HasOne(f => f.Employee)
                      .WithMany(e => e.Fines)
                      .HasForeignKey(f => f.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Bonus>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Reason).IsRequired().HasMaxLength(500);
                entity.Property(b => b.Amount).HasColumnType("decimal(18,2)");

                entity.HasOne(b => b.Employee)
                      .WithMany(e => e.Bonuses)
                      .HasForeignKey(b => b.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Backup>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Name).IsRequired().HasMaxLength(200);
                entity.Property(b => b.Data).IsRequired();
            });
        }
    }
}