using System;
using TasqueManager.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace TasqueManager.Infrastructure
{
    public class DatabaseContext: DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        /// <summary>
        /// Задачи.
        /// </summary>
        public DbSet<Assignment> Assignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Assignment>().Property(c => c.Title).HasMaxLength(100);
            modelBuilder.Entity<Assignment>().Property(c => c.Description).HasMaxLength(100);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        }

        public void BeginTransaction()
        {
            Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            Database.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            Database.RollbackTransaction();
        }
    }
}
