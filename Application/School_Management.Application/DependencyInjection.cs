using Microsoft.Extensions.DependencyInjection;
using School_Management.Application.Services;
using School_Management.Core.Interfaces.Application;

namespace School_Management.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton<IPhotoDeleteService, PhotoDeleteService>();
            services.AddSingleton<IPhotoFetchService, PhotoFetchService>();
            services.AddSingleton<IPhotoUploadService, PhotoUploadService>();
            services.AddSingleton<IUserSessionService, UserSessionService>();
            services.AddSingleton<IUserValidationService, UserValidationService>();
            services.AddSingleton<ICandidateService, CandidateService>();

            return services;
        }
    }
}
