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
using UHub.Web.Extensions.Account;
using UHub.Web.Infrastructure;

/// <summary>
/// �û���Ϣ ʹ�� ASP.NET Core Identity ���� EF Core
/// IdentityServer4 ��֤��Ȩ�����Ϣ ʹ�� IdentityServer4 ���� EF Core
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

            #region ���� ASP.NET Core Identity ʹ�� EF Core
            services.AddDbContext<ApplicationDbContext>(options =>
                //options.UseSqlite(connectionString));
                options.UseMySQL(connectionString));

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            #endregion

            #region ���� IdentityServer4 ʹ�� EF Core
            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                options.EmitStaticAudienceClaim = true;
            })
                // �����ڴ��еĳ�ʼ��Ԥ����Ϣ
                //.AddInMemoryIdentityResources(Config.IdentityResources)
                //.AddInMemoryApiScopes(Config.ApiScopes)
                //.AddInMemoryClients(Config.Clients)
                // IdentityServer4 �� EF Core ��ȡ�û�
                // IdentityServer4 �� ASP.NET Core Identity ���
                .AddAspNetIdentity<ApplicationUser>()
                // IdentityServer4 �� EF Core ���
                // IdentityServer4 �� clients, resources����Ϣ����EF Core
                // ConfigurationDbContext - used for configuration data such as clients, resources, and scopes
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseMySQL(connectionString);
                })
                // PersistedGrantDbContext - used for temporary operational data such as authorization codes, and refresh tokens
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseMySQL(connectionString);
                });
            #endregion

            #region ���� IdentityServer4 ǩ������Կ
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

            #region ��֤����
            services.AddAuthentication()
                .AddExternalLogin(Configuration);
            #endregion

            // ����SameSiteCookie����
            services.AddSameSiteCookiePolicy();

            // ��� IOption ����
            services.AddOptions();
            services.Configure<UHubOptions>(Configuration.GetSection("UHub"));

            // ��Ӻ�̨����
            //services.AddBackgroundServices();

            // ���Ӧ��֪ͨ���������
            services.AddScoped<AppNoticeTaskManager>();

            // ������ݿ��ʼ����
            services.AddScoped<DbInitializer>();

            #region �����Ȩ-������Щ�˹����̨Admin
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
            // ���� app.UseAuthentication(); ��Ϊ UseIdentityServer() �ڲ����˴˲���
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}