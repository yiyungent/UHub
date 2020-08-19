using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace UHub.Web.BackgroundServices
{
    public static class BackgroundServicesHelper
    {
        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            //services.AddScoped(typeof(IHostedService), typeof(TimeBackgroundService));
            services.AddHostedService<AppNoticeTaskBackgroundService>(); // 以这种方式注入就是单例

            return services;
        }
    }
}
