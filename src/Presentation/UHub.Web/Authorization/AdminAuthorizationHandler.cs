using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using UHub.Web.Config;

namespace UHub.Web.Authorization
{
    public class AdminAuthorizationHandler : AuthorizationHandler<AdminRequirement>
    {
        private readonly IOptionsMonitor<UHubOptions> _options;

        public AdminAuthorizationHandler(IOptionsMonitor<UHubOptions> options)
        {
            // fixed bug: 改了appsettings.json并没有引起 IOptions<UHubOptions> 的变化, 难道这个是单例? 不是配置改了,会重新加载吗?
            // 已解决: 因为 IOptions是单例
            // 参考: https://www.cnblogs.com/dingsblog/p/6761804.html
            // IOptions 无热更新，正常读取配置
            // IOptionsSnapshot 作用域容器配置热更新使用它
            // IOptionsMonitor 单例容器配置热更新使用它
            _options = options;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       AdminRequirement requirement)
        {
            int[] adminUserIds = _options.CurrentValue.AdminUserIds.ToArray();
            var claim = context.User.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Subject);
            if (claim != null && int.TryParse(claim.Value, out int userId))
            {
                if (adminUserIds.Contains(userId))
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}