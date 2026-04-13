using Microsoft.Extensions.DependencyInjection;
using School_Management.Core.Interfaces.Presentation;
using School_Management.Presentation.Shared.Services;

namespace School_Management.Presentation.Shared
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentationShared(this IServiceCollection services)
        {
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<IFileDialogService, FileDialogService>();
            services.AddSingleton<ISoundService, SoundService>();
            services.AddSingleton<ICameraService, CameraService>();

            return services;
        }
    }
}
