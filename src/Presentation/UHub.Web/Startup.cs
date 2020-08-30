// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.IO;
using IdentityServer4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using UHub.Web.BackgroundServices;
using UHub.Web.Config;
using UHub.Data;
using UHub.Data.Models;
using UHub.Web.Authorization;
using UHub.Web.Extensions;
using UHub.Web.Infrastructure;

/// <summary>
/// 用户信息 使用 ASP.NET Core Identity 存于 EF Core
/// IdentityServer4 认证授权相关信息 使用 IdentityServer4 存于 EF Core
/// </summary>
namespace UHub.Web
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            string connectionString = Configuration.GetConnectionString("DefaultConnection");

            #region 配置 ASP.NET Core Identity 使用 EF Core
            services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(connectionString,
                        // Fixed Bug EF Migration : Your target project 'UHub.Web' doesn't match your migrations assembly 'UHub.Data'. Either change your target project or change your migrations assembly.
                        // Change your migrations assembly by using DbContextOptionsBuilder.E.g.options.UseSqlServer(connection, b => b.MigrationsAssembly("UHub.Web")). By default, the migrations assembly is the assembly containing the DbContext.
                        //Change your target project to the migrations project by using the Package Manager Console's Default project drop-down list, or by executing "dotnet ef" from the directory containing the migrations project.
                        b => b.MigrationsAssembly("UHub.Web")));

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            #endregion

            #region 配置 IdentityServer4 使用 EF Core
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                options.EmitStaticAudienceClaim = true;
            })
                // 存于内存中的初始化预制信息
                //.AddInMemoryIdentityResources(Config.IdentityResources)
                //.AddInMemoryApiScopes(Config.ApiScopes)
                //.AddInMemoryClients(Config.Clients)
                // IdentityServer4 从 EF Core 中取用户
                // IdentityServer4 与 ASP.NET Core Identity 结合
                .AddAspNetIdentity<ApplicationUser>()
                // IdentityServer4 与 EF Core 结合
                // IdentityServer4 的 clients, resources等信息存于EF Core
                // ConfigurationDbContext - used for configuration data such as clients, resources, and scopes
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseSqlite(connectionString,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                // PersistedGrantDbContext - used for temporary operational data such as authorization codes, and refresh tokens
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseSqlite(connectionString,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                });
            #endregion

            #region 配置 IdentityServer4 签名用秘钥
            // not recommended for production - you need to store your key material somewhere secure
            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else if (Environment.IsProduction())
            {
                string fileName = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "UHubKey.jwk");
                builder.AddDeveloperSigningCredential(filename: fileName);
            }
            #endregion

            #region 认证配置
            services.AddAuthentication()
                   .AddGoogle(options =>
                   {
                       options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                       // register your IdentityServer with Google at https://console.developers.google.com
                       // enable the Google+ API
                       // set the redirect URI to https://localhost:5001/signin-google
                       options.ClientId = "copy client ID from Google here";
                       options.ClientSecret = "copy client secret from Google here";
                   });
            #endregion

            // 配置SameSiteCookie策略
            services.AddSameSiteCookiePolicy();

            // 添加 IOption 配置
            services.AddOptions();
            services.Configure<UHubOptions>(Configuration.GetSection("UHub"));

            // 添加后台任务
            services.AddBackgroundServices();

            // 添加应用通知任务管理器
            services.AddScoped<AppNoticeTaskManager>();

            #region 添加授权-允许哪些人管理后台Admin
            services.AddSingleton<IAuthorizationHandler, AdminAuthorizationHandler>();

            services.AddAuthorization(options =>
                {
                    options.AddPolicy("Admin",
                        policy =>
                        {
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(new AdminRequirement());
                        }
                    );
                });
            #endregion
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            // UseCookiePolicy(): Before UseAuthentication or anything else that writes cookies.
            app.UseCookiePolicy();
            // 无需 app.UseAuthentication(); 因为 UseIdentityServer() 内部做了此操作
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}