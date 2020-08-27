using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UHub.Web.Models.Client
{
    public class ClientInputModel
    {
        public int Id { get; set; }

        public string ClientId { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string AllowedGrantTypes { get; set; }

        public string AllowedScopes { get; set; }

        public string AllowedCorsOrigins { get; set; }

        public string ClientSecret { get; set; }

        public string PostLogoutRedirectUris { get; set; }

        public string RedirectUris { get; set; }

        public bool RequireConsent { get; set; }

        public bool AllowAccessTokensViaBrowser { get; set; }

        public bool AlwaysIncludeUserClaimsInIdToken { get; set; }
    }
}
