using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using UHub.Web.Models.Client;
using UHub.Web.Models.Common;
using Secret = IdentityServer4.Models.Secret;

namespace UHub.Web.Controllers
{
    public class ClientController : Controller
    {
        #region Fields

        private readonly ConfigurationDbContext _configurationDbContext;

        #endregion

        #region Ctor

        public ClientController(ConfigurationDbContext configurationDbContext)
        {
            _configurationDbContext = configurationDbContext;
        }
        #endregion

        #region Actions

        #region 客户端列表
        public async Task<IActionResult> Index()
        {
            IList<ClientViewModel> viewModel = new List<ClientViewModel>();
            var dbModel = await _configurationDbContext.Clients
                    .Include(d => d.AllowedGrantTypes)
                    .Include(d => d.AllowedScopes)
                    .Include(d => d.AllowedCorsOrigins)
                    .Include(d => d.RedirectUris)
                    .Include(d => d.PostLogoutRedirectUris)
                    .ToListAsync();
            foreach (var dbClient in dbModel)
            {
                var clientModel = dbClient.ToModel();
                viewModel.Add(new ClientViewModel
                {
                    Id = dbClient.Id,
                    ClientId = clientModel.ClientId,
                    RequireConsent = clientModel.RequireConsent,
                    PostLogoutRedirectUris = clientModel.PostLogoutRedirectUris,
                    AllowedCorsOrigins = clientModel.AllowedCorsOrigins,
                    Description = clientModel.Description,
                    DisplayName = clientModel.ClientName,
                    AllowAccessTokensViaBrowser = clientModel.AllowAccessTokensViaBrowser,
                    AllowedGrantTypes = clientModel.AllowedGrantTypes,
                    AllowedScopes = clientModel.AllowedScopes,
                    AlwaysIncludeUserClaimsInIdToken = clientModel.AlwaysIncludeUserClaimsInIdToken,
                    RedirectUris = clientModel.RedirectUris,
                    CreateTime = dbClient.Created.ToString("yyyy-MM-dd HH:mm"),
                    LastUpdateTime = dbClient.Updated?.ToString("yyyy-MM-dd HH:mm") ?? dbClient.Created.ToString("yyyy-MM-dd HH:mm"),
                    LogoUri = dbClient.LogoUri
                });
            }

            return View(viewModel);
        }
        #endregion

        #region 查看
        public async Task<IActionResult> Details(int id)
        {
            ClientViewModel viewModel = null;
            // 记得在 .ToModel() 前查询时 Include(), 不然 .ToModel() 关联属性为空
            var dbModel = await _configurationDbContext.Clients
                .Include(d => d.AllowedGrantTypes)
                .Include(d => d.AllowedScopes)
                .Include(d => d.AllowedCorsOrigins)
                .Include(d => d.RedirectUris)
                .Include(d => d.PostLogoutRedirectUris)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dbModel != null)
            {
                var clientModel = dbModel.ToModel();
                viewModel = new ClientViewModel
                {
                    Id = dbModel.Id,
                    ClientId = clientModel.ClientId,
                    AllowedCorsOrigins = clientModel.AllowedCorsOrigins,
                    Description = clientModel.Description,
                    PostLogoutRedirectUris = clientModel.PostLogoutRedirectUris,
                    RequireConsent = clientModel.RequireConsent,
                    LogoUri = clientModel.LogoUri,
                    AllowedGrantTypes = clientModel.AllowedGrantTypes,
                    AllowedScopes = clientModel.AllowedScopes,
                    AllowAccessTokensViaBrowser = clientModel.AllowAccessTokensViaBrowser,
                    RedirectUris = clientModel.RedirectUris,
                    AlwaysIncludeUserClaimsInIdToken = clientModel.AlwaysIncludeUserClaimsInIdToken,
                    CreateTime = dbModel.Created.ToString("yyyy-MM-dd HH:mm"),
                    LastUpdateTime = dbModel.Updated?.ToString("yyyy-MM-dd HH:mm") ?? dbModel.Created.ToString("yyyy-MM-dd HH:mm"),
                    DisplayName = clientModel.ClientName
                };
            }

            return View(viewModel);
        }
        #endregion

        #region 创建

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["AllGrantTypes"] = new string[]{
                "authorization_code",
                "password",
                "client_credentials",
                "implict",
                "hybrid"
            };

            return View();
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel>> Create(ClientInputModel inputModel)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                // TODO: 效验
                #region 效验

                #endregion

                // InputModel => IdentityServer4.Models
                IdentityServer4.Models.Client clientModel = new IdentityServer4.Models.Client()
                {
                    ClientId = inputModel.ClientId,
                    ClientName = inputModel.DisplayName,
                    // TODO: 这里预留, 但视图目前不支持多选, 授权类型始终只能选择一种
                    AllowedGrantTypes = inputModel.AllowedGrantTypes?.Split(","),
                    AllowedScopes = inputModel.AllowedScopes?.Split(","),
                    Description = inputModel.Description,
                    AllowedCorsOrigins = inputModel.AllowedCorsOrigins?.Split(","),
                    ClientSecrets = new List<Secret>()
                    {
                        new Secret(inputModel.ClientSecret.Sha256())
                    },
                    PostLogoutRedirectUris = inputModel.PostLogoutRedirectUris?.Split(","),
                    RedirectUris = inputModel.RedirectUris?.Split(","),
                    RequireConsent = inputModel.RequireConsent,
                    AllowAccessTokensViaBrowser = inputModel.AllowAccessTokensViaBrowser,
                    AlwaysIncludeUserClaimsInIdToken = inputModel.AlwaysIncludeUserClaimsInIdToken
                };

                // 保存到数据库
                await _configurationDbContext.Clients.AddAsync(clientModel.ToEntity());
                await _configurationDbContext.SaveChangesAsync();

                responseModel.code = 1;
                responseModel.message = "创建成功 ";
            }
            catch (Exception ex)
            {
                responseModel.code = -1;
                responseModel.message = "创建失败: " + ex.Message;
            }

            return await Task.FromResult(responseModel);
        }
        #endregion



        #endregion

    }
}
