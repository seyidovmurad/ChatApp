using ChatClient.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Data
{
    public class AppDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source = DESKTOP-J450DMQ; Initial Catalog = TestDb; Integrated Security = True;");
        }

        public DbSet<Participant> Participants { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<Doctor> Doctors { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>()
                .HasKey(m => new { m.UserId, m.DoctorId });
            modelBuilder.Entity<Message>()
                .HasOne(m => m.User)
                .WithMany(u => u.Chatter)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Doctor)
                .WithMany(d => d.Chatter)
                .HasForeignKey(m => m.DoctorId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
