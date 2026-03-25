using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using School_Management.Core.Interfaces;
using School_Management.Infrastructure.Data;
using School_Management.Infrastructure.Repositories;
using School_Management.Infrastructure.Services;

namespace School_Management.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Register DbContext as singleton for WPF
            services.AddDbContext<SchoolDbContext>(options =>
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", false, true)
                    .Build();

                string connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found.");

                // Use Npgsql with connection pooling for better performance
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                });
            });

            // Register Repositories
            services.AddScoped<IAttendanceRepository, AttendanceRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<ICandidateRepository, CandidateRepository>();
            services.AddScoped<IClassRepository, ClassRepository>();
            services.AddScoped<IClassSubjectRepository, ClassSubjectRepository>();
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IExamRepository, ExamRepository>();
            services.AddScoped<IGenerationRepository, GenerationRepository>();
            services.AddScoped<IGradeRepository, GradeRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IScoreRepository, ScoreRepository>();
            services.AddScoped<ISkillRepository, SkillRepository>();
            services.AddScoped<IStudentClassRepository, StudentClassRepository>();
            services.AddScoped<IStudentQRRepository, StudentQRRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<ISubjectRepository, SubjectRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            // Register Infrastructure Services
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IS3Service, S3Service>();

            return services;
        }
    }
}
