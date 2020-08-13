using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace UHub.Web.Controllers
{
    public class InstallController : Controller
    {
        /// <summary>
        /// ConfigurationDbContext - used for configuration data such as clients, resources, and scopes
        /// </summary>
        private readonly PersistedGrantDbContext _persistedGrantDbContext;

        /// <summary>
        /// PersistedGrantDbContext - used for temporary operational data such as authorization codes, and refresh tokens
        /// </summary>
        private readonly ConfigurationDbContext _configurationDbContext;

        private readonly IConfiguration _configuration;


        public InstallController(PersistedGrantDbContext persistedGrantDbContext, ConfigurationDbContext configurationDbContext, IConfiguration configuration)
        {
            _persistedGrantDbContext = persistedGrantDbContext;
            _configurationDbContext = configurationDbContext;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            #region ASP.NET Core Identity 用户信息 初始化
            string connStr = _configuration.GetConnectionString("DefaultConnection");
            SeedData.EnsureSeedData(connStr);
            #endregion

            #region IdentityServer4 客户端等配置信息 初始化
            _configurationDbContext.Database.Migrate();
            _persistedGrantDbContext.Database.Migrate();

            var context = _configurationDbContext;
            if (!context.Clients.Any())
            {
                foreach (var client in Config.Clients)
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.IdentityResources.Any())
            {
                foreach (var resource in Config.IdentityResources)
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.ApiScopes.Any())
            {
                foreach (var resource in Config.ApiScopes)
                {
                    context.ApiScopes.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            #endregion

            return Json(new { message = "安装成功" });
        }
    }
}
