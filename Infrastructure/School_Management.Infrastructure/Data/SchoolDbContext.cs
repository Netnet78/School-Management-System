using Microsoft.EntityFrameworkCore;
using School_Management.Core.Models;

namespace School_Management.Infrastructure.Data
{
    public class SchoolDbContext : DbContext
    {
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<StudentQR> StudentQRs { get; set; }
        public DbSet<StudentClass> StudentClasses { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Score> Scores { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Generation> Generations { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<ClassSubject> ClassSubjects { get; set; }
        public DbSet<Class> Classes { get; set; }

        public SchoolDbContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>()
                .HasIndex(s => s.CandidateId)
                .IsUnique();
            modelBuilder.Entity<Candidate>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Candidate>()
                .HasOne(c => c.Student)
                .WithOne(s => s.Candidate)
                .HasForeignKey<Student>(s => s.CandidateId);

            modelBuilder.Entity<Attendance>()
                .Property(a => a.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Candidate>()
                .Property(e => e.Gender)
                .HasConversion<string>();
            modelBuilder.Entity<Candidate>()
                .Property(e => e.StayType)
                .HasConversion<string>();

            modelBuilder.Entity<Employee>()
                .Property(e => e.Gender)
                .HasConversion<string>();
            modelBuilder.Entity<Employee>()
                .Property(e => e.MaritalStatus)
                .HasConversion<string>();


            base.OnModelCreating(modelBuilder);
        }
    }
}
