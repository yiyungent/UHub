using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using UHubApi.AspNetCore.Models;

namespace UHubApi.AspNetCore.Middlewares
{
    public class RenameUserMiddleware : UHubApiMiddleware<RenameUserModel>
    {
        public RenameUserMiddleware(RequestDelegate next, IOptions<UHubApiOptions> optionsAccessor, IUHubApiApp uHubApiApp) : base(next, optionsAccessor, uHubApiApp)
        { }

        public override AppNoticeResponseModel ExecuteApi(RenameUserModel model)
        {
            return _uHubApiApp.RenameUser(model);
        }
    }
}
