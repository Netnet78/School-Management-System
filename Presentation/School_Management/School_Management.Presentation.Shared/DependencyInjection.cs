using Microsoft.Extensions.DependencyInjection;
using School_Management.Presentation.Shared.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace School_Management.Presentation.Shared
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentationShared(this IServiceCollection services)
        {
            services.AddSingleton<IMessageService, MessageService>();

            return services;
        }
    }
}
