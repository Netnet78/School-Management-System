using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolManagement.Core.Shared.Configurations;
using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Features.Accessments.Repositories;
using SchoolManagement.Infrastructure.Features.Attendances.Repositories;
using SchoolManagement.Infrastructure.Features.AuditLogs.Repositories;
using SchoolManagement.Infrastructure.Features.Auth.Repositories;
using SchoolManagement.Infrastructure.Features.Candidates.Repositories;
using SchoolManagement.Infrastructure.Features.Classes.Repositories;
using SchoolManagement.Infrastructure.Features.Departments.Repositories;
using SchoolManagement.Infrastructure.Features.Employees.Repositories;
using SchoolManagement.Infrastructure.Features.Exams.Repositories;
using SchoolManagement.Infrastructure.Features.Files.Services;
using SchoolManagement.Infrastructure.Features.Generations.Repositories;
using SchoolManagement.Infrastructure.Features.Grades.Repositories;
using SchoolManagement.Infrastructure.Features.Notifications.Repositories;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;
using SchoolManagement.Infrastructure.Features.Reports.Export;
using SchoolManagement.Infrastructure.Features.Reports.Export.Rendering;
using SchoolManagement.Infrastructure.Features.Skills.Repositories;
using SchoolManagement.Infrastructure.Features.Students.Repositories;
using SchoolManagement.Infrastructure.Features.Subjects.Repositories;
using SchoolManagement.Infrastructure.Interceptors;

namespace SchoolManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Register DbContext as scoped for WPF
            services.AddDbContext<SchoolDbContext>((sp, options) =>
            {
                string connectionString = SecretHelper.GetValueFromEnv("DB_CONNECTION");

                // Use Npgsql with connection pooling for better performance
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                });

                AuditSaveChangesInterceptor interceptor = sp.GetRequiredService<AuditSaveChangesInterceptor>();
                options.AddInterceptors(interceptor);
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
            services.AddScoped<IAccessmentRepository, ScoreRepository>();
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

            // Report exporters
            services.AddTransient<IReportExporter, ExcelExporter>();
            services.AddTransient<IReportExporter, PdfExporter>();
            services.AddScoped<IS3Service, S3Service>();
            services.AddScoped<IPhotoSyncService, PhotoSyncService>();

            // Report renderers
            services.AddTransient<IExcelRenderer, DefaultExcelTemplateRenderer>();
            services.AddTransient<IExcelRenderer, StudentRosterRenderer>();
            services.AddTransient<ICardPdfRenderer, StudentCardRenderer>();
            services.AddTransient<IPdfRenderer, TablePdfRenderer>();
            services.AddTransient<IPdfRenderer, CardPdfRenderer>();

            return services;
        }
    }
}
