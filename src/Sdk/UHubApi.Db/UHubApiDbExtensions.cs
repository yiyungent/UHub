using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UHubApi.Db
{
    public static class UHubApiDbExtensions
    {
        public static void AddPluginFramework(this IServiceCollection services)
        {
            using (var serviceProvider = services.BuildServiceProvider())
            {
                using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var configuration = scope.ServiceProvider.GetService<IConfiguration>();

                    string connStr = configuration["UHub:ConnStr"];
                    services.AddDbContext<UHubApiDbContext>(options =>
                        options.UseMySQL(connStr));
                }
            }



        }
    }
}
