using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UHub.Web.Models.IdentityResource
{
    public class IdentityResourceInputModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public bool Required { get; set; }

        public bool ShowInDiscoveryDocument { get; set; }

        public string UserClaims { get; set; }
    }
}
