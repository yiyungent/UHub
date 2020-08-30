using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UHub.Web.Models.Client;
using UHub.Web.Models.IdentityResource;

namespace UHub.Web.Controllers
{
    [Authorize("Admin")]
    public class IdentityResourceController : Controller
    {
        #region Fields

        private readonly ConfigurationDbContext _configurationDbContext;

        #endregion

        #region Ctor

        public IdentityResourceController(ConfigurationDbContext configurationDbContext)
        {
            _configurationDbContext = configurationDbContext;
        }
        #endregion

        #region Actions

        #region IdentityResource列表
        public async Task<IActionResult> Index()
        {
            IList<IdentityResourceViewModel> viewModel = new List<IdentityResourceViewModel>();
            var dbModel = await _configurationDbContext.IdentityResources
                    .Include(d => d.UserClaims)
                    .Include(d => d.Properties)
                .ToListAsync();
            foreach (var dbIdentityResource in dbModel)
            {
                var identityResourceModel = dbIdentityResource.ToModel();
                viewModel.Add(new IdentityResourceViewModel
                {
                    Id = dbIdentityResource.Id,
                    Name = identityResourceModel.Name,
                    DisplayName = identityResourceModel.DisplayName,
                    Description = identityResourceModel.Description,
                    Required = identityResourceModel.Required,
                    ShowInDiscoveryDocument = identityResourceModel.ShowInDiscoveryDocument,
                    UserClaims = identityResourceModel.UserClaims,
                    CreateTime = dbIdentityResource.Created.ToString("yyyy-MM-dd HH:mm"),
                    LastUpdateTime = dbIdentityResource.Updated?.ToString("yyyy-MM-dd HH:mm") ?? dbIdentityResource.Created.ToString("yyyy-MM-dd HH:mm"),
                });
            }

            return View(viewModel);
        }
        #endregion

        #region 查看
        public async Task<IActionResult> Details(int id)
        {
            IdentityResourceViewModel viewModel = null;
            // 记得在 .ToModel() 前查询时 Include(), 不然 .ToModel() 关联属性为空
            var dbModel = await _configurationDbContext.IdentityResources
                .Include(d => d.UserClaims)
                .Include(d => d.Properties)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dbModel != null)
            {
                var identityResourceModel = dbModel.ToModel();
                viewModel = new IdentityResourceViewModel
                {
                    Id = dbModel.Id,
                    Name = identityResourceModel.Name,
                    DisplayName = identityResourceModel.DisplayName,
                    Description = identityResourceModel.Description,
                    Required = identityResourceModel.Required,
                    ShowInDiscoveryDocument = identityResourceModel.ShowInDiscoveryDocument,
                    UserClaims = identityResourceModel.UserClaims,
                    CreateTime = dbModel.Created.ToString("yyyy-MM-dd HH:mm"),
                    LastUpdateTime = dbModel.Updated?.ToString("yyyy-MM-dd HH:mm") ?? dbModel.Created.ToString("yyyy-MM-dd HH:mm"),
                };
            }

            return View(viewModel);
        }
        #endregion

        #endregion
    }
}
