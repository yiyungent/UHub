using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace UHub.Web.Attributes
{
    public class ApiNameAttribute : Attribute
    {
        public string DisplayName { get; set; }

        public string Description { get; set; }

        public ApiNameAttribute(string displayName)
        {
            this.DisplayName = displayName;
        }
    }
}
