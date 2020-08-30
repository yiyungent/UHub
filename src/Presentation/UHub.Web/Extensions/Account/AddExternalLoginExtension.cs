using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using UHub.Web.Config;

namespace UHub.Web.Extensions.Account
{
    public static class AddExternalLoginExtension
    {
        /// <summary>
        /// 自动注册外部登陆组件
        /// </summary>
        /// <param name="builder"></param>
        public static void AddExternalLogin(this AuthenticationBuilder builder, IConfiguration configuration)
        {
            UHubOptions options = new UHubOptions();
            configuration.GetSection("UHub").Bind(options);

            // 所有启用的外部登陆
            IList<UHubOptions.ExternalLoginModel> enabledExternalLogins = options.ExternalLogins?.Where(m => m.Enabled)?.ToList();
            if (enabledExternalLogins != null && enabledExternalLogins.Count >= 1)
            {
                // 反射找外部登陆类, 并添加
                Type needInterface = typeof(IExternalLogin);
                string path = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
                Assembly[] referencedAssemblies = System.IO.Directory.GetFiles(path, "*.dll").Select(Assembly.LoadFrom).ToArray();
                List<Type> types = referencedAssemblies.SelectMany(m => m.ExportedTypes).ToList();
                List<Type> externalLoginType = types.Where(m =>
                      (!m.IsAbstract && !m.IsInterface)
                      &&
                      (m.BaseType == needInterface || m.GetInterfaces().Contains(needInterface))
                   ).ToList();
                foreach (Type type in externalLoginType)
                {
                    IExternalLogin login = Activator.CreateInstance(type) as IExternalLogin;
                    var loginConfig = enabledExternalLogins.FirstOrDefault(m => m.Name == login.Name);
                    if (loginConfig != null)
                    {
                        builder = login.Add(builder, clientId: loginConfig.ClientId, clientSecret: loginConfig.ClientSecret);
                    }
                }


            }
        }
    }
}
