using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UHub.Web.Attributes;

namespace UHub.Web.Api
{
    [ApiName("添加新用户")]
    public class AddUserModel : AppNoticePostDataModel
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }
    }
}
