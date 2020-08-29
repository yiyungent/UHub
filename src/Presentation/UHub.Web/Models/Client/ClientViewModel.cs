using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UHub.Web.Models.Client
{
    public class ClientViewModel
    {
        public int Id { get; set; }

        public string ClientId { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string CreateTime { get; set; }

        public string LastUpdateTime { get; set; }

        public string LogoUri { get; set; }

        /// <summary>
        /// 授权类型
        /// </summary>
        public ICollection<string> AllowedGrantTypes { get; set; }

        /// <summary>
        /// 允许范围
        /// </summary>
        public ICollection<string> AllowedScopes { get; set; }

        public ICollection<string> AllowedCorsOrigins { get; set; }

        /// <summary>
        /// AccessToken 是否可以通过浏览器返回
        /// </summary>
        public bool AllowAccessTokensViaBrowser { get; set; }

        /// <summary>
        /// where to redirect to after login
        /// </summary>
        public ICollection<string> RedirectUris { get; set; }

        /// <summary>
        /// where to redirect to after logout
        /// </summary>
        public ICollection<string> PostLogoutRedirectUris { get; set; }

        /// <summary>
        /// In addition to an id_token, an access_token was requested. No claims other than sub are included in the id_token. To obtain more user claims, either use the user info endpoint or set AlwaysIncludeUserClaimsInIdToken on the client configuration.
        /// 开启后, AspNetCoreMvc 通过 User.Claims 能获取更多信息, 列如 profile: family_name...
        /// </summary>
        public bool AlwaysIncludeUserClaimsInIdToken { get; set; }

        /// <summary>
        /// 是否需要同意授权页面
        /// </summary>
        public bool RequireConsent { get; set; }

        /// <summary>
        /// 是否需要 RefreshToken
        /// PS: 允许离线访问即返回 RefreshToken, 用于刷新 AccessToken
        /// </summary>
        public bool AllowOfflineAccess { get; set; }
    }
}
