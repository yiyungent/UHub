﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UHub.Web.Models.Client;
using UHub.Web.Models.Common;
using Secret = IdentityServer4.Models.Secret;

namespace UHub.Web.Controllers
{
    [Authorize("Admin")]
    public class ClientController : Controller
    {
        #region Fields

        private readonly ConfigurationDbContext _configurationDbContext;

        #endregion

        #region Properties

        public string[] AllGrantTypes
        {
            get
            {
                return new string[]{
                    string.Join(",",IdentityServer4.Models.GrantTypes.Code),
                    string.Join(",",IdentityServer4.Models.GrantTypes.CodeAndClientCredentials),
                    string.Join(",",IdentityServer4.Models.GrantTypes.Implicit),
                    string.Join(",",IdentityServer4.Models.GrantTypes.ImplicitAndClientCredentials),
                    string.Join(",", IdentityServer4.Models.GrantTypes.Hybrid),
                    string.Join(",",IdentityServer4.Models.GrantTypes.HybridAndClientCredentials),
                    string.Join(",",IdentityServer4.Models.GrantTypes.ResourceOwnerPassword),
                    string.Join(",",IdentityServer4.Models.GrantTypes.ResourceOwnerPasswordAndClientCredentials),
                    string.Join(",",IdentityServer4.Models.GrantTypes.ClientCredentials),
                    // TODO: Bug: 这个值为, 不知道干啥的 urn:ietf:params:oauth:grant-type:device_code
                    //string.Join(",",IdentityServer4.Models.GrantTypes.DeviceFlow),
                };
            }
        }

        public string[] AllScopes
        {
            get
            {
                return new string[]{
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Address,
                    IdentityServerConstants.StandardScopes.Email,
                    IdentityServerConstants.StandardScopes.Phone,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                };
            }
        }

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
                    DisplayName = clientModel.ClientName,
                    AllowOfflineAccess = clientModel.AllowOfflineAccess
                };
            }

            return View(viewModel);
        }
        #endregion

        #region 创建

        [HttpGet]
        public IActionResult Create()
        {
            // 注意: 不是所有授权类型都能同时支持, 应改为单选, 选项中有固定可行的单配方案(eg: CodeAndClientCredentials)
            // Fixed Bug: InvalidOperationException: Grant types list cannot contain both implicit and authorization_code
            ViewData["AllGrantTypes"] = AllGrantTypes;
            ViewData["AllScopes"] = AllScopes;

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
                // 1.授权类型只能来自选项
                // eg: 若选项是两项: CodeAndClientCredentials => authorization_code,client_credentials
                string selectedGrantTypes = inputModel.AllowedGrantTypes;
                if (!AllGrantTypes.Contains(selectedGrantTypes))
                {
                    responseModel.code = -1;
                    responseModel.message = "创建失败: 不存在此授权类型";
                    return await Task.FromResult(responseModel);
                }

                #endregion

                // InputModel => IdentityServer4.Models
                IdentityServer4.Models.Client clientModel = new IdentityServer4.Models.Client()
                {
                    ClientId = inputModel.ClientId,
                    ClientName = inputModel.DisplayName,
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
                    AlwaysIncludeUserClaimsInIdToken = inputModel.AlwaysIncludeUserClaimsInIdToken,
                    AllowOfflineAccess = inputModel.AllowOfflineAccess
                };

                // 保存到数据库
                var dbModel = clientModel.ToEntity();
                // TODO: 注意: 1.发现使用 DateTime.UtcNow 时间不正确，不知道为什么，明明推荐用这个统一时间 2.就算不手动赋值, 最后也会有创建时间, 更新时间, 而内部用的就是UtcNow
                dbModel.Created = DateTime.Now;
                await _configurationDbContext.Clients.AddAsync(dbModel);
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

        #region 编辑
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ClientViewModel viewModel = null;
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
                    DisplayName = clientModel.ClientName,
                    AllowOfflineAccess = clientModel.AllowOfflineAccess
                };

            }

            ViewData["AllGrantTypes"] = AllGrantTypes;
            ViewData["AllScopes"] = AllScopes;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel>> Edit(ClientInputModel inputModel)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                // TODO: 效验
                #region 效验

                if (inputModel == null || inputModel.Id == 0)
                {
                    responseModel.code = -1;
                    responseModel.message = "更新失败: 不存在此客户端";
                    return await Task.FromResult(responseModel);
                }

                // 授权类型只能来自选项
                // eg: 若选项是两项: CodeAndClientCredentials => authorization_code,client_credentials
                string selectedGrantTypes = inputModel.AllowedGrantTypes;
                if (!AllGrantTypes.Contains(selectedGrantTypes))
                {
                    responseModel.code = -1;
                    responseModel.message = "编辑失败: 不存在此授权类型";
                    return await Task.FromResult(responseModel);
                }
                #endregion

                // 覆盖更新: 先从数据库查出原有数据
                var dbModel = await _configurationDbContext.Clients
                    .Include(d => d.AllowedGrantTypes)
                    .Include(d => d.AllowedScopes)
                    .Include(d => d.AllowedCorsOrigins)
                    .Include(d => d.RedirectUris)
                    .Include(d => d.PostLogoutRedirectUris)
                    .FirstOrDefaultAsync(m => m.Id == inputModel.Id);
                if (dbModel == null)
                {
                    responseModel.code = -1;
                    responseModel.message = "更新失败: 不存在此客户端";
                    return await Task.FromResult(responseModel);
                }
                // 注意: IdentityServer4.Models 没有 Id 属性，数据库操作时只能用于创建，不能用于更新
                // 秘钥不为空, 覆盖旧秘钥
                #region 覆盖秘钥
                if (!string.IsNullOrEmpty(inputModel.ClientSecret?.Trim()))
                {
                    // 注意: 约定: 一个客户端只能有一个秘钥
                    dbModel.ClientSecrets = new List<ClientSecret>();

                    // TODO: bug: 修改秘钥, 每次都是新增加秘钥, 不会删除已有秘钥
                    // 下方这样，也无法解决，每次修改还是会增加秘钥，而不会删除原来的
                    //dbModel.ClientSecrets.Clear();
                    //_configurationDbContext.Clients.Update(dbModel);
                    //await _configurationDbContext.SaveChangesAsync();

                    dbModel.ClientSecrets.Add(new ClientSecret()
                    {
                        ClientId = dbModel.Id,
                        Created = DateTime.Now,
                        Type = "SharedSecret",
                        Value = new Secret(inputModel.ClientSecret.Sha256()).Value
                    });
                }
                #endregion

                // InputModel => dbModel
                #region 普通属性赋值
                dbModel.ClientId = inputModel.ClientId;
                dbModel.ClientName = inputModel.DisplayName;
                dbModel.Description = inputModel.Description;
                dbModel.RequireConsent = inputModel.RequireConsent;
                dbModel.AllowAccessTokensViaBrowser = inputModel.AllowAccessTokensViaBrowser;
                dbModel.AlwaysIncludeUserClaimsInIdToken = inputModel.AlwaysIncludeUserClaimsInIdToken;
                dbModel.AllowOfflineAccess = inputModel.AllowOfflineAccess;
                dbModel.Updated = DateTime.Now;
                #endregion

                // 关联属性赋值
                #region 关联属性赋值
                dbModel.AllowedGrantTypes = new List<ClientGrantType>();
                if (!string.IsNullOrEmpty(inputModel.AllowedGrantTypes))
                {
                    inputModel.AllowedGrantTypes.Split(",").Where(m => m != "" && m != null).ToList().ForEach(q =>
                        {
                            dbModel.AllowedGrantTypes.Add(new IdentityServer4.EntityFramework.Entities.ClientGrantType()
                            {
                                ClientId = dbModel.Id,
                                GrantType = q
                            });
                        });
                }

                dbModel.AllowedScopes = new List<ClientScope>();
                if (!string.IsNullOrEmpty(inputModel.AllowedScopes))
                {
                    inputModel.AllowedScopes.Split(",").Where(m => m != "" && m != null).ToList().ForEach(q =>
                    {
                        dbModel.AllowedScopes.Add(new IdentityServer4.EntityFramework.Entities.ClientScope()
                        {
                            ClientId = dbModel.Id,
                            Scope = q
                        });
                    });
                }

                dbModel.AllowedCorsOrigins = new List<ClientCorsOrigin>();
                if (!string.IsNullOrEmpty(inputModel.AllowedCorsOrigins))
                {
                    inputModel.AllowedCorsOrigins.Split(",").Where(m => m != "" && m != null).ToList().ForEach(q =>
                    {
                        dbModel.AllowedCorsOrigins.Add(new IdentityServer4.EntityFramework.Entities.ClientCorsOrigin()
                        {
                            ClientId = dbModel.Id,
                            Origin = q
                        });
                    });
                }

                dbModel.PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUri>();
                if (!string.IsNullOrEmpty(inputModel.PostLogoutRedirectUris))
                {
                    inputModel.PostLogoutRedirectUris.Split(",").Where(m => m != "" && m != null).ToList().ForEach(q =>
                    {
                        dbModel.PostLogoutRedirectUris.Add(new IdentityServer4.EntityFramework.Entities.ClientPostLogoutRedirectUri()
                        {
                            ClientId = dbModel.Id,
                            PostLogoutRedirectUri = q
                        });
                    });
                }

                dbModel.RedirectUris = new List<ClientRedirectUri>();
                if (!string.IsNullOrEmpty(inputModel.RedirectUris))
                {
                    inputModel.RedirectUris.Split(",").Where(m => m != "" && m != null).ToList().ForEach(q =>
                    {
                        dbModel.RedirectUris.Add(new IdentityServer4.EntityFramework.Entities.ClientRedirectUri()
                        {
                            ClientId = dbModel.Id,
                            RedirectUri = q
                        });
                    });
                }
                #endregion

                // 保存到数据库
                _configurationDbContext.Clients.Update(dbModel);
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

        #region 删除

        public async Task<ActionResult<ResponseModel>> Delete(int id)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                var dbModel = await _configurationDbContext.Clients.FirstOrDefaultAsync(m => m.Id == id);
                if (dbModel == null)
                {
                    responseModel.code = -1;
                    responseModel.message = "删除失败: 不存在此客户端";
                    return await Task.FromResult(responseModel);
                }

                _configurationDbContext.Clients.Remove(dbModel);
                await _configurationDbContext.SaveChangesAsync();

                responseModel.code = 1;
                responseModel.message = "删除成功";
            }
            catch (Exception ex)
            {
                responseModel.code = -1;
                responseModel.message = "删除失败: " + ex.Message;
            }

            return await Task.FromResult(responseModel);
        }

        #endregion

        #endregion

    }
}
