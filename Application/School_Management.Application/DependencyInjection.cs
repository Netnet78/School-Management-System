using Microsoft.Extensions.DependencyInjection;
using School_Management.Application.Services;
using School_Management.Core.Interfaces;

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

            return services;
        }
    }
}
