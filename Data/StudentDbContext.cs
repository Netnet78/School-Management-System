using Microsoft.EntityFrameworkCore;
using New_Student_Management.Models;

namespace New_Student_Management.Data
{
    public class StudentDbContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5432;Database=NewStudentsDB;Username=postgres;Password=Internet@2008"
            );
            base.OnConfiguring(optionsBuilder);
        }
    }
}
