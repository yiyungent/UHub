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
using Serilog;

namespace UHub.Web.Controllers
{
    public class InstallController : Controller
    {
        #region Fields
        private readonly DbInitializer _dbInitializer;
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
        public InstallController(DbInitializer dbInitializer, IWebHostEnvironment environment)
        {
            this._dbInitializer = dbInitializer;
            this.Environment = environment;
        }
        #endregion

        #region Actions
        public IActionResult Index()
        {
            if (IsInstalled)
            {
                Log.Debug("已安装, 若需重新安装, 请删除 App_Data/install.lock");
                return Json(new { message = "已安装, 若需重新安装, 请删除 App_Data/install.lock" });
            }

            try
            {
                Log.Debug("开始安装");

                this._dbInitializer.Initialize();

                IsInstalled = true;
                Log.Debug("安装成功");
            }
            catch (Exception e)
            {
                IsInstalled = false;
                Log.Debug(e, "安装失败");
            }

            return Json(new { message = "安装成功" });
        }
        #endregion
    }
}
