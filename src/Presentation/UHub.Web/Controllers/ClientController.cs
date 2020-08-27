using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UHub.Web.Models.Client;

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

        #endregion

    }
}
