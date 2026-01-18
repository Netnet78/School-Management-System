using Microsoft.EntityFrameworkCore;
using School_Management.Core.Models;

namespace School_Management.Infrastructure.Data
{
    public class StudentDbContext : DbContext
    {
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5432;Database=SchoolDB;Username=postgres;Password=Internet@2008"
            );
            base.OnConfiguring(optionsBuilder);
        }
    }
}
