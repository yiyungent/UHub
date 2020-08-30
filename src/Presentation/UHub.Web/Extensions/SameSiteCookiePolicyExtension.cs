using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace UHub.Web.Extensions
{
    /// <summary>
    /// fixed bug: 非HTTPS下: 无法登陆, 因为 Set-Cookie 时, 被浏览器不允许写入, Chrome新版本中有此问题
    /// Set-Cookie: idsrv.session=C97A9C77108CFD6713B117B79FF462F5; path=/; samesite=none
    /// samesite 参考 http://www.ruanyifeng.com/blog/2019/09/cookie-samesite.html
    /// https://docs.microsoft.com/zh-cn/aspnet/core/security/samesite?view=aspnetcore-3.1
    /// https://devblogs.microsoft.com/aspnet/upcoming-samesite-cookie-changes-in-asp-net-and-asp-net-core/
    /// Antiforgery token validation failed.: https://www.cnblogs.com/dudu/p/10959557.html
    /// </summary>
    public static class SameSiteCookiePolicyExtension
    {
        #region AddSameSiteCookiePolicy
        public static IServiceCollection AddSameSiteCookiePolicy(this IServiceCollection services)
        {
            // 修复后: HTTP: Set-Cookie: idsrv.session=1E69EFACB5B29A6A62C638B9C7AC85CF; path=/
            // HTTPS: set-cookie: idsrv.session=609CF6E966CBC4214FA8F74C49F4658E; path=/; secure
            // PS: idsrv.session 是来自IdentityServer4登陆成功后设置的Cookie(Session)
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.OnAppendCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });

            // Antiforgery token validation failed.: https://www.cnblogs.com/dudu/p/10959557.html
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = ".UHub.Antiforgery";
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            return services;
        } 
        #endregion

        #region CheckSameSite
        private static void CheckSameSite(HttpContext httpContext, CookieOptions options)
        {
            if (options.SameSite == SameSiteMode.None)
            {
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
                // Use your User Agent library of choice here.
                if (DisallowsSameSiteNone(userAgent))
                {
                    // For .NET Core < 3.1 set SameSite = (SameSiteMode)(-1)
                    options.SameSite = SameSiteMode.Unspecified;
                }
            }
        }
        #endregion

        #region DisallowsSameSiteNone
        private static bool DisallowsSameSiteNone(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
            {
                return false;
            }

            // Cover all iOS based browsers here. This includes:
            // - Safari on iOS 12 for iPhone, iPod Touch, iPad
            // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
            // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
            // All of which are broken by SameSite=None, because they use the iOS networking stack
            if (userAgent.Contains("CPU iPhone OS 12") || userAgent.Contains("iPad; CPU OS 12"))
            {
                return true;
            }

            // Cover Mac OS X based browsers that use the Mac OS networking stack. This includes:
            // - Safari on Mac OS X.
            // This does not include:
            // - Chrome on Mac OS X
            // Because they do not use the Mac OS networking stack.
            if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
                userAgent.Contains("Version/") && userAgent.Contains("Safari"))
            {
                return true;
            }

            // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
            // and none in this range require it.
            // Note: this covers some pre-Chromium Edge versions, 
            // but pre-Chromium Edge does not require SameSite=None.

            // Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 Safari/537.36
            //if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
            // 这里修改下, 只要包含Chrome, 原来的不可用
            if (userAgent.Contains("Chrome"))
            {
                return true;
            }

            return false;
        } 
        #endregion


    }
}
