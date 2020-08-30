using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace UHub.Web.Controllers
{
    public class InstallController : Controller
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

        private readonly IConfiguration _configuration;
        #endregion

        #region Properties
        public IWebHostEnvironment Environment { get; }

        public bool IsInstalled
        {
            get
            {
                if (Environment.IsDevelopment())
                {
                    // 开发环境下无视安装锁, 随意安装
                    return false;
                }
                bool isInstalled = true;
                try
                {
                    string installLockFilePath =
                        System.IO.Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "install.lock");
                    if (!System.IO.File.Exists(installLockFilePath))
                    {
                        isInstalled = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return isInstalled;
            }
            set
            {
                if (Environment.IsDevelopment())
                {
                    // 开发环境下不写安装锁, 随意安装
                    return;
                }
                string installLockFilePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "App_Data/install.lock");
                if (value)
                {
                    System.IO.File.Create(installLockFilePath);
                }
                else
                {
                    System.IO.File.Delete(installLockFilePath);
                }
            }
        }
        #endregion

        #region Ctor
        public InstallController(PersistedGrantDbContext persistedGrantDbContext, ConfigurationDbContext configurationDbContext, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _persistedGrantDbContext = persistedGrantDbContext;
            _configurationDbContext = configurationDbContext;
            _configuration = configuration;
            Environment = environment;
        }
        #endregion

        public IActionResult Index()
        {
            if (IsInstalled)
            {
                return Json(new { message = "已安装, 若需重新安装, 请删除 App_Data/install.lock" });
            }

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
                foreach (var client in Ids4Config.Clients)
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.IdentityResources.Any())
            {
                foreach (var resource in Ids4Config.IdentityResources)
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.ApiScopes.Any())
            {
                foreach (var resource in Ids4Config.ApiScopes)
                {
                    context.ApiScopes.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            #endregion

            IsInstalled = true;

            return Json(new { message = "安装成功" });
        }
    }
}
