using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Shared.Configurations;
using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Interceptors;
using SchoolManagement.Infrastructure.Repositories;
using SchoolManagement.Infrastructure.Services;

namespace SchoolManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Register DbContext as singleton for WPF
            services.AddDbContext<SchoolDbContext>(options =>
            {
                string connectionString = SecretHelper.GetValueFromEnv("DB_CONNECTION");

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

            // Register Interceptors / Middleware
            services.AddScoped<AuditSaveChangesInterceptor>();

            // Register Infrastructure Services
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IS3Service, S3Service>();
            services.AddScoped<IPhotoSyncService, PhotoSyncService>();

            return services;
        }
    }
}
