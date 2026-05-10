using Microsoft.Extensions.DependencyInjection;
using School_Management.Application.Policies;
using School_Management.Application.Services;
using School_Management.Application.Workers;
using School_Management.Core.Interfaces.Application;

namespace School_Management.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IPhotoDeleteService, PhotoDeleteService>();
            services.AddScoped<IPhotoFetchService, PhotoFetchService>();
            services.AddScoped<IPhotoUploadService, PhotoUploadService>();
            services.AddScoped<IUserSessionService, UserSessionService>();
            services.AddScoped<IUserValidationService, UserValidationService>();
            services.AddScoped<ICandidateService, CandidateService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IClassService, ClassService>();
            services.AddScoped<IClassSubjectService, ClassSubjectService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IExamService, ExamService>();
            services.AddScoped<IGenerationService, GenerationService>();
            services.AddScoped<IGradeService, GradeService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IScoreService, ScoreService>();
            services.AddScoped<ISkillService, SkillService>();
            services.AddScoped<IStudentClassService, StudentClassService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IStudentQRService, StudentQRService>();
            services.AddScoped<ISubjectService, SubjectService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();

            // Permissions handlers
            services.AddScoped<IAuthorizationHandler, AttendanceAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, StudentAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, ClassAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, EmployeeAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, CandidateAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, GradeAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, ExamAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, ScoreAuthorizationHandler>();

            // Background workers
            services.AddScoped<FileSyncBackgroundWorker>();

            return services;
        }
    }
}
