using System;
using System.Collections.Generic;
using System.Text;

namespace UHubApi.AspNetCore.Models
{
    public class RenameUserModel
    {
        public string OldUserName { get; set; }

        public string NewUserName { get; set; }
    }
}
