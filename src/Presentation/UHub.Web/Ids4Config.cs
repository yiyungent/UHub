// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Linq;

namespace UHub.Web
{
    public static class Ids4Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                //new ApiScope("Remember.Core WebApi", "Remember.Core WebApi")
            };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                // machine to machine client
                new Client
                {
                    ClientId = "mm",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    ClientName = "示例1: machine to machine client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    // scopes that client has access to
                    AllowedScopes =  new List<string> {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                    },
                },
                
                // interactive ASP.NET Core MVC client
                new Client
                {
                    ClientId = "mvc",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    ClientName="示例2: interactive ASP.NET Core MVC client",

                    AllowedGrantTypes = GrantTypes.Code,
                    
                    // where to redirect to after login
                    RedirectUris = { "https://localhost:5003/signin-oidc" },

                    // where to redirect to after logout
                    PostLogoutRedirectUris = { "https://localhost:5003/signout-callback-oidc" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                    },

                    // In addition to an id_token, an access_token was requested. No claims other than sub are included in the id_token. To obtain more user claims, either use the user info endpoint or set AlwaysIncludeUserClaimsInIdToken on the client configuration.
                    // 开启后, AspNetCoreMvc 通过 User.Claims 能获取更多信息, 列如 profile: family_name...
                    AlwaysIncludeUserClaimsInIdToken = true,
                },

                #region 不需要
		                //new Client
                //{
                //    ClientId = "webapi",
                //    ClientSecrets = { new Secret("secret".Sha256()) },

                //    AllowedGrantTypes = new string[] {
                //        GrantTypes.ResourceOwnerPassword.First(),
                //        GrantTypes.Code.First()
                //    },
                    
                //    // where to redirect to after login
                //    RedirectUris = { "https://localhost:5002/signin-oidc" },

                //    // where to redirect to after logout
                //    PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

                //    AllowedScopes = new List<string>
                //    {
                //        IdentityServerConstants.StandardScopes.OpenId,
                //        IdentityServerConstants.StandardScopes.Profile,
                //        // 允许访问 天气信息, 实体信息
                //        "WeatherInfo",
                //        "IdentityInfo"
                //    }
                //},

                // new Client
                //{
                //    ClientId = "remember-app",
                //    ClientSecrets = { new Secret("remember-app Secret".Sha256()) },

                //    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    
                //    // where to redirect to after login
                //    RedirectUris = { "https://localhost:5002/signin-oidc" },

                //    // where to redirect to after logout
                //    PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

                //    AlwaysIncludeUserClaimsInIdToken = true,

                //    AllowedScopes = new List<string>
                //    {
                //        IdentityServerConstants.StandardScopes.OpenId,
                //        IdentityServerConstants.StandardScopes.Profile,
                //        "Remember.Core WebApi"
                //    },
                //    AllowedCorsOrigins = { "https://localhost:5005" }
                //},

                // new Client
                // {
                //     ClientId = "Remember.Core Admin SPA",
                //     ClientSecrets = { new Secret("Remember.Core Admin SPA Secret".Sha256()) },

                //     AllowedGrantTypes = GrantTypes.Implicit,

                //     // AccessToken 是否可以通过浏览器返回
                //     AllowAccessTokensViaBrowser = true,
                    
                //     // where to redirect to after login
                //     RedirectUris = { "https://localhost:5002/signin-oidc" },

                //     // where to redirect to after logout
                //     PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

                //     AlwaysIncludeUserClaimsInIdToken = true,

                //     AllowedScopes = new List<string>
                //     {
                //         IdentityServerConstants.StandardScopes.OpenId,
                //         IdentityServerConstants.StandardScopes.Profile,
                //         "Remember.Core WebApi"
                //     },
                //     AllowedCorsOrigins = { "https://localhost:5005" }
                // }, 
	#endregion
            };
    }
}