using System;
using Microsoft.EntityFrameworkCore;
using Test.Common;
using Test.Data.Entities;

namespace Test.Data
{
    public class TestDbContext : DbContext
    {
        public DbSet<Task> Tasks { get; set; }

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Task>()
                .Property(e => e.State)
                .HasConversion(
                    v => v.ToString(),
                    v => (ETaskState) Enum.Parse(typeof(ETaskState), v));
        }
    }
}