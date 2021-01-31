using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UHub.Data;
using UHub.Data.Models;
using UHub.Web.Api;
using UHub.Web.Attributes;
using UHub.Web.Tasks;

namespace UHub.Web
{
    public class DbInitializer
    {

        #region Fields
        /// <summary>
        /// ConfigurationDbContext - used for configuration data such as clients, resources, and scopes
        /// </summary>
        private readonly PersistedGrantDbContext _persistedGrantDbContext;

        /// <summary>
        /// PersistedGrantDbContext - used for temporary operational data such as authorization codes, and refresh tokens
        /// </summary>
        private readonly ConfigurationDbContext _configurationDbContext;

        private readonly ApplicationDbContext _applicationDbContext;

        private readonly UserManager<ApplicationUser> _userManager;
        #endregion

        #region Ctor
        public DbInitializer(PersistedGrantDbContext persistedGrantDbContext, ConfigurationDbContext configurationDbContext, ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager)
        {
            this._persistedGrantDbContext = persistedGrantDbContext;
            this._configurationDbContext = configurationDbContext;
            this._applicationDbContext = applicationDbContext;
            this._userManager = userManager;
        }
        #endregion

        public void Initialize()
        {
            #region 清空数据库
            try
            {
                // TODO: 安装前 先 清空数据库
                //this._applicationDbContext.Database.EnsureDeleted();

                //Log.Debug("清空数据库: 成功");
            }
            catch (Exception e)
            {
                //Log.Debug(e, "清空数据库: 失败");
            }
            #endregion

            #region 创建 表结构
            try
            {
                string applicationSql = this._applicationDbContext.Database.GenerateCreateScript();
                this._applicationDbContext.Database.ExecuteSqlRaw(applicationSql);

                string configurationSql = this._configurationDbContext.Database.GenerateCreateScript();
                this._configurationDbContext.Database.ExecuteSqlRaw(configurationSql);

                string persistedGrantSql = this._persistedGrantDbContext.Database.GenerateCreateScript();
                this._persistedGrantDbContext.Database.ExecuteSqlRaw(persistedGrantSql);

                Log.Debug("创建表结构: 成功");
            }
            catch (Exception e)
            {
                Log.Debug(e, "创建表结构: 失败");
            }
            #endregion

            #region 初始化 ASP.NET Core Identity 用户信息
            try
            {
                var alice = new ApplicationUser
                {
                    UserName = "alice",
                    Email = "AliceSmith@email.com",
                    EmailConfirmed = true,
                };
                var result = this._userManager.CreateAsync(alice, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = this._userManager.AddClaimsAsync(alice, new Claim[]{
                    new Claim(JwtClaimTypes.Name, "Alice Smith"),
                    new Claim(JwtClaimTypes.GivenName, "Alice"),
                    new Claim(JwtClaimTypes.FamilyName, "Smith"),
                    new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                var bob = new ApplicationUser
                {
                    UserName = "bob",
                    Email = "BobSmith@email.com",
                    EmailConfirmed = true
                };
                result = this._userManager.CreateAsync(bob, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = this._userManager.AddClaimsAsync(bob, new Claim[]{
                    new Claim(JwtClaimTypes.Name, "Bob Smith"),
                    new Claim(JwtClaimTypes.GivenName, "Bob"),
                    new Claim(JwtClaimTypes.FamilyName, "Smith"),
                    new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                    new Claim("location", "somewhere")
                }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                Log.Debug("初始化: ASP.NET Core Identity 用户信息: 成功");
            }
            catch (Exception e)
            {
                Log.Debug(e, "初始化: ASP.NET Core Identity 用户信息: 失败");
            }
            #endregion

            #region 初始化 应用通信 任务信息TaskInfo
            try
            {
                Type[] types = Assembly.GetExecutingAssembly().GetTypes().Where(m => m.BaseType == typeof(AppNoticePostDataModel)).ToArray();
                foreach (var type in types)
                {
                    TaskInfo taskInfo = new TaskInfo
                    {
                        Name = type.Name.Replace("Model", ""),
                        DisplayName = type.GetCustomAttribute<ApiNameAttribute>().DisplayName,
                        Description = type.GetCustomAttribute<ApiNameAttribute>().Description,
                        TaskType = AppNoticeTask.TaskType,
                    };
                    this._applicationDbContext.TaskInfo.Add(taskInfo);
                }
                this._applicationDbContext.SaveChanges();

                Log.Debug("初始化: TaskInfo: 成功");
            }
            catch (Exception e)
            {
                Log.Debug(e, "初始化: TaskInfo: 失败");
            }
            #endregion

            #region 初始化 IdentityServer4 客户端等配置信息
            try
            {
                foreach (var client in Ids4InitConfig.Clients)
                {
                    _configurationDbContext.Clients.Add(client.ToEntity());
                }
                _configurationDbContext.SaveChanges();

                foreach (var resource in Ids4InitConfig.IdentityResources)
                {
                    _configurationDbContext.IdentityResources.Add(resource.ToEntity());
                }
                _configurationDbContext.SaveChanges();

                foreach (var resource in Ids4InitConfig.ApiScopes)
                {
                    _configurationDbContext.ApiScopes.Add(resource.ToEntity());
                }
                _configurationDbContext.SaveChanges();

                Log.Debug("初始化: IdentityServer4 客户端等配置信息: 成功");
            }
            catch (Exception e)
            {
                Log.Debug(e, "初始化: IdentityServer4 客户端等配置信息: 失败");
            }
            #endregion
        }
    }



    #region IdentityServer4 初始配置
    public static class Ids4InitConfig
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId()
                {
                    DisplayName = "用户标识",
                    Description = "你的用户标识"
                },
                new IdentityResources.Profile()
                {
                    DisplayName = "个人信息",
                    Description = "你的个人信息"
                }
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("Remember.Core.WebApi", "Remember.Core.WebApi")
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

                // Remember.Core Admin SPA
                new Client
                 {
                     ClientId = "Remember.Core Admin SPA",
                     ClientSecrets = { new Secret("Remember.Core Admin SPA Secret".Sha256()) },

                     AllowedGrantTypes = GrantTypes.Implicit,

                     // AccessToken 是否可以通过浏览器返回
                     AllowAccessTokensViaBrowser = true,
                    
                     // where to redirect to after login
                     RedirectUris = { "http://localhost:9528/oidc/callback" },

                     // where to redirect to after logout
                     PostLogoutRedirectUris = { "http://localhost:9528/oidc/logoutCallback" },

                     AlwaysIncludeUserClaimsInIdToken = true,

                     AllowedScopes = new List<string>
                     {
                         IdentityServerConstants.StandardScopes.OpenId,
                         IdentityServerConstants.StandardScopes.Profile,
                         "Remember.Core.WebApi"
                     },
                     AllowedCorsOrigins = { "http://localhost:9528" }
                 },
            };
    }
    #endregion


}
