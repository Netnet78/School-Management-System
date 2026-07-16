using Microsoft.EntityFrameworkCore;
using SchoolManagement.Core.Features.Assessments.Models;
using SchoolManagement.Core.Features.Auth.Models;

namespace SchoolManagement.Infrastructure.Data
{
    public class SchoolDbContext : DbContext
    {
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<SubjectComponent> SubjectComponents { get; set; }
        public DbSet<SubjectMapper> SubjectMappers { get; set; }
        public DbSet<StudentQR> StudentQRs { get; set; }
        public DbSet<StudentClass> StudentClasses { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentPhoto> StudentPhotos { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
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
        public DbSet<EmployeePhoto> EmployeePhotos { get; set; }

        public SchoolDbContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SubjectComponent>()
                .HasIndex(s => s.Name)
                .IsUnique();

            modelBuilder.Entity<Subject>()
                .Property(s => s.MaxScore)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Skill>()
                .HasOne(s => s.Department)
                .WithOne(d => d.Skill)
                .HasForeignKey<Department>(d => d.Id);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.StudentQR)
                .WithOne(q => q.Student)
                .HasForeignKey<StudentQR>(q => q.Id);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Photo)
                .WithOne(p => p.Employee)
                .HasForeignKey<EmployeePhoto>(p => p.Id);

            modelBuilder.Entity<Candidate>()
                .HasOne(c => c.Photo)
                .WithOne(p => p.Student)
                .HasForeignKey<StudentPhoto>(p => p.Id);

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.CandidateId)
                .IsUnique();
            modelBuilder.Entity<Candidate>()
                .Property(c => c.FullName)
                .HasComputedColumnSql($"\"{nameof(Candidate.LastName)}\" || ' ' || \"{nameof(Candidate.FirstName)}\"", true);
            modelBuilder.Entity<Candidate>()
                .Property(c => c.LatinFullName)
                .HasComputedColumnSql($"\"{nameof(Candidate.LatinLastName)}\" || ' ' || \"{nameof(Candidate.LatinFirstName)}\"", true);
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

            modelBuilder.Entity<User>()
                .HasIndex(u => u.EmployeeId)
                .IsUnique();
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<User>(u => u.EmployeeId);
            modelBuilder.Entity<Employee>()
                .Property(e => e.Gender)
                .HasConversion<string>();
            modelBuilder.Entity<Employee>()
                .Property(e => e.MaritalStatus)
                .HasConversion<string>();

            modelBuilder.Entity<ClassSubject>(entity =>
            {
                entity.Property(cs => cs.TeacherId)
                    .HasColumnName("EmployeeId");

                entity.HasOne(cs => cs.Class)
                    .WithMany(c => c.Subjects)
                    .HasForeignKey(cs => cs.ClassId);

                entity.HasOne(cs => cs.Teacher)
                    .WithMany(e => e.ClassSubjects)
                    .HasForeignKey(cs => cs.TeacherId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cs => cs.Subject)
                    .WithMany(s => s.ClassSubjects)
                    .HasForeignKey(cs => cs.SubjectId);
            });

            modelBuilder.HasDefaultSchema("public");

            base.OnModelCreating(modelBuilder);
        }
    }
}
