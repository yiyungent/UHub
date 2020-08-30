using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UHub.Web.Config
{
    public class UHubOptions
    {
        public int SyncRate { get; set; }

        public int TaskMaxExecCount { get; set; }

        public int TaskExpireAfter { get; set; }

        /// <summary>
        /// 拥有管理UHub后台功能的用户ID列表
        /// </summary>
        public IList<int> AdminUserIds { get; set; }

        /// <summary>
        /// 外部第三方登录
        /// </summary>
        public IList<ExternalLoginModel> ExternalLogins { get; set; }

        public class ExternalLoginModel
        {
            public string Name { get; set; }

            public bool Enabled { get; set; }

            public string ClientId { get; set; }

            public string ClientSecret { get; set; }
        }
    }
}
