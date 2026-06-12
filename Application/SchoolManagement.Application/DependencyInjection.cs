using Microsoft.Extensions.DependencyInjection;
using SchoolManagement.Application.Features.Reports;
using SchoolManagement.Application.Features.Reports.Contracts;

namespace SchoolManagement.Application
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
            services.AddScoped<IAccessmentService, ScoreService>();
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
            services.AddScoped<IAuthorizationHandler, AuditLogAuthorizationHandler>();

            // Background workers
            services.AddScoped<FileSyncBackgroundWorker>();

            // Report registry
            services.AddSingleton<IReportRegistry, ReportRegistry>();

            return services;
        }
    }
}
