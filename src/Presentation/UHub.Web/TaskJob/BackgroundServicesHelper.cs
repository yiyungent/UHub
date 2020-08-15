using Microsoft.Extensions.DependencyInjection;

namespace UHub.Web.TaskJob
{
    public static class BackgroundServicesHelper
    {
        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            //services.AddTransient(typeof(IHostedService), typeof(TimeBackgroundService));
            services.AddHostedService<TimeBackgroundService>(); // 以这种方式注入就是单例

            return services;
        }
    }
}
