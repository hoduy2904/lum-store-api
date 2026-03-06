using LumStoreAPI.Tasks.BackgroundServices;
using Microsoft.Extensions.DependencyInjection;

namespace LumStoreAPI.Tasks
{
    public static class LumStoreTaskConfiguration
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection RegisterTasks()
            {
                services.AddHostedService<EmailSenderBackgroundService>();
                return services;
            }
        }
    }
}
