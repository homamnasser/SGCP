using SGCP.Models;
using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SGCP.Context
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Government> Governments { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<ComplaintType> ComplaintTypes { get; set; }
        public DbSet<ComplaintAttachment> ComplaintAttachments { get; set; }
        public DbSet<ComplaintHistory> ComplaintHistories { get; set; }
        public DbSet<ComplaintLock> ComplaintLocks { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Phone)
                .IsUnique();

            modelBuilder.Entity<Government>()
                .HasIndex(u => u.Name)
                .IsUnique();


            // ------------------------------
            // User relationships
            // ------------------------------
            modelBuilder.Entity<User>()
                .HasOne(u => u.Government)
                .WithMany(g => g.Employees)
                .HasForeignKey(u => u.GovernmentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Complaints)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.ComplaintHistories)
                .WithOne(h => h.Employee)
                .HasForeignKey(h => h.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------------------
            // Complaint relationships
            // ------------------------------
            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Government)
                .WithMany(e => e.Complaints)
                .HasForeignKey(c => c.GovernmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Complaint>()
                .HasMany(c => c.Attachments)
                .WithOne(a => a.Complaint)
                .HasForeignKey(a => a.ComplaintId);

            modelBuilder.Entity<Complaint>()
                .HasMany(c => c.History)
                .WithOne(h => h.Complaint)
                .HasForeignKey(h => h.ComplaintId);

            // ------------------------------
            // Complaint Type
            // ------------------------------
            modelBuilder.Entity<ComplaintType>()
                .HasMany(t => t.Complaints)
                .WithOne(c => c.Type)
                .HasForeignKey(c => c.TypeId);


            // ------------------------------
            // Complaint Lock
            // ------------------------------
            modelBuilder.Entity<ComplaintLock>()
                .HasOne(l => l.Complaint)
                .WithOne()
                .HasForeignKey<ComplaintLock>(l => l.ComplaintId);

            modelBuilder.Entity<ComplaintLock>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------------------
            // Notifications
            // ------------------------------
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ------------------------------
            // Role relationships
            // ------------------------------
            modelBuilder.Entity<Role>()
                .HasMany(u => u.Users)
                .WithOne(c => c.Role)
                .HasForeignKey(c => c.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
