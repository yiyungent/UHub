using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UHub.Web.Attributes;

namespace UHub.Web.Api
{
    [ApiName("删除用户")]
    public class DeleteUserModel : AppNoticePostDataModel
    {
        public int UID { get; set; }

        public string UserName { get; set; }
    }
}
