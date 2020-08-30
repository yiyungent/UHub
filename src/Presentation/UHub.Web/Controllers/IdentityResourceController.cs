using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UHub.Web.Models.Client;
using UHub.Web.Models.Common;
using UHub.Web.Models.IdentityResource;

namespace UHub.Web.Controllers
{
    [Authorize("Admin")]
    public class IdentityResourceController : Controller
    {
        #region Fields

        private readonly ConfigurationDbContext _configurationDbContext;

        #endregion

        #region Properties

        public string[] AllUserClaims
        {
            get
            {
                return new string[]{
                    "updated_at",
                    "locale",
                    "zoneinfo",
                    "birthdate",
                    "gender",
                    "website",
                    "picture",
                    "profile",
                    "preferred_username",
                    "nickname",
                    "middle_name",
                    "given_name",
                    "family_name",
                    "name",
                    "sub"
                };
            }
        }

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

        #region 编辑
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            IdentityResourceViewModel viewModel = null;
            var dbModel = await _configurationDbContext.IdentityResources
                .Include(d => d.UserClaims)
                .Include(d => d.Properties)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dbModel != null)
            {
                var identityResourceModel = dbModel.ToModel();
                viewModel = new IdentityResourceViewModel()
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

            ViewData["AllUserClaims"] = AllUserClaims;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel>> Edit(IdentityResourceInputModel inputModel)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                // TODO: 效验
                #region 效验

                if (inputModel == null || inputModel.Id == 0)
                {
                    responseModel.code = -1;
                    responseModel.message = "更新失败: 不存在此身份资源";
                    return await Task.FromResult(responseModel);
                }

                #endregion

                // 覆盖更新: 先从数据库查出原有数据
                var dbModel = await _configurationDbContext.IdentityResources
                    .Include(d => d.UserClaims)
                    .Include(d => d.Properties)
                    .FirstOrDefaultAsync(m => m.Id == inputModel.Id);
                if (dbModel == null)
                {
                    responseModel.code = -1;
                    responseModel.message = "更新失败: 不存在此身份资源";
                    return await Task.FromResult(responseModel);
                }

                // InputModel => dbModel
                #region 普通属性赋值
                dbModel.Name = inputModel.Name;
                dbModel.DisplayName = inputModel.DisplayName;
                dbModel.Description = inputModel.Description;
                dbModel.Required = inputModel.Required;
                dbModel.ShowInDiscoveryDocument = inputModel.ShowInDiscoveryDocument;
                dbModel.Updated = DateTime.Now;
                #endregion

                // 关联属性赋值
                #region 关联属性赋值
                dbModel.UserClaims = new List<IdentityResourceClaim>();
                if (!string.IsNullOrEmpty(inputModel.UserClaims))
                {
                    inputModel.UserClaims.Split(",").Where(m => m != "" && m != null).ToList().ForEach(q =>
                    {
                        dbModel.UserClaims.Add(new IdentityServer4.EntityFramework.Entities.IdentityResourceClaim()
                        {
                            IdentityResourceId = dbModel.Id,
                            Type = q
                        });
                    });
                }


                #endregion

                // 保存到数据库
                _configurationDbContext.IdentityResources.Update(dbModel);
                await _configurationDbContext.SaveChangesAsync();

                responseModel.code = 1;
                responseModel.message = "更新成功 ";
            }
            catch (Exception ex)
            {
                responseModel.code = -1;
                responseModel.message = "更新失败: " + ex.Message;
            }

            return await Task.FromResult(responseModel);
        }
        #endregion

        #endregion
    }
}
