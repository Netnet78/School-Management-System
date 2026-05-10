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
                string connectionString = Env.Get("DB_CONNECTION");

                // Use Npgsql with connection pooling for better performance
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                });
            }, ServiceLifetime.Scoped);

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
            services.AddScoped<IStudentPhotoRepository, StudentPhotoRepository>();
            services.AddScoped<IEmployeePhotoRepository, EmployeePhotoRepository>();

            // Register Infrastructure Services
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IS3Service, S3Service>();
            services.AddScoped<IPhotoSyncService, PhotoSyncService>();

            return services;
        }
    }
}
