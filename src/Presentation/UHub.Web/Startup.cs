// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using UHub.Web.Data;
using UHub.Web.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using UHub.Web.TaskJob;

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
                    options.UseSqlite(connectionString));

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            #endregion

            #region ���� IdentityServer4 ʹ�� EF Core
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

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();

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

            // ��Ӻ�̨����
            //services.AddBackgroundServices();
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
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}