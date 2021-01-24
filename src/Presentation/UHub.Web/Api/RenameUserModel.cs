using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UHub.Web.Attributes;

namespace UHub.Web.Api
{
    [ApiName("用户重命名")]
    public class RenameUserModel : AppNoticePostDataModel
    {
        public string OldUserName { get; set; }

        public string NewUserName { get; set; }
    }
}
