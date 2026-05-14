using Microsoft.Extensions.DependencyInjection;
using SchoolManagement.Presentation.Shared.Services;

namespace SchoolManagement.Presentation.Shared
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentationShared(this IServiceCollection services)
        {
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<IFileDialogService, FileDialogService>();
            services.AddSingleton<ISoundService, SoundService>();
            services.AddSingleton<ICameraService, CameraService>();
            services.AddSingleton<IFrameProcessingService, FrameProcessingService>();
            services.AddSingleton<IQRScannerService, QRScannerService>();
            services.AddSingleton<IDispatcherService, DispatcherService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ILoadingService, LoadingService>();

            return services;
        }
    }
}
