using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using School_Management.Core.Helpers;
using School_Management.Core.Interfaces.Infrastructure;
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
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .Build();

                string connectionString = Env.Get("DB_CONNECTION");

                // Use Npgsql with connection pooling for better performance
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                });
            });

            // Register Repositories
            services.AddSingleton<IAttendanceRepository, AttendanceRepository>();
            services.AddSingleton<IAuditLogRepository, AuditLogRepository>();
            services.AddSingleton<ICandidateRepository, CandidateRepository>();
            services.AddSingleton<IClassRepository, ClassRepository>();
            services.AddSingleton<IClassSubjectRepository, ClassSubjectRepository>();
            services.AddSingleton<IDepartmentRepository, DepartmentRepository>();
            services.AddSingleton<IEmployeeRepository, EmployeeRepository>();
            services.AddSingleton<IExamRepository, ExamRepository>();
            services.AddSingleton<IGenerationRepository, GenerationRepository>();
            services.AddSingleton<IGradeRepository, GradeRepository>();
            services.AddSingleton<INotificationRepository, NotificationRepository>();
            services.AddSingleton<IPermissionRepository, PermissionRepository>();
            services.AddSingleton<IRoleRepository, RoleRepository>();
            services.AddSingleton<IScoreRepository, ScoreRepository>();
            services.AddSingleton<ISkillRepository, SkillRepository>();
            services.AddSingleton<IStudentClassRepository, StudentClassRepository>();
            services.AddSingleton<IStudentQRRepository, StudentQRRepository>();
            services.AddSingleton<IStudentRepository, StudentRepository>();
            services.AddSingleton<ISubjectRepository, SubjectRepository>();
            services.AddSingleton<IUserRepository, UserRepository>();

            // Register Infrastructure Services
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IS3Service, S3Service>();

            return services;
        }
    }
}
