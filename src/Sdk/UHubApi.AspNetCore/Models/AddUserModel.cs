using System;
using System.Collections.Generic;
using System.Text;

namespace UHubApi.AspNetCore.Models
{
    public class AddUserModel
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public long CreateTime { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Description { get; set; }
    }
}
