using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace UHub.Web.Extensions.Account
{
    public class ExternalLoginGitHub : IExternalLogin
    {
        public string Name => "GitHub";

        public AuthenticationBuilder Add(AuthenticationBuilder builder, string clientId, string clientSecret)
        {
            builder.AddGitHub(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
            });

            return builder;
        }
    }
}
