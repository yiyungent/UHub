using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using UHubApi.AspNetCore.Models;

namespace UHubApi.AspNetCore.Middlewares
{
    public class DeleteUserMiddleware : UHubApiMiddleware<DeleteUserModel>
    {
        public DeleteUserMiddleware(RequestDelegate next, IOptions<UHubApiOptions> optionsAccessor, IUHubApiApp uHubApiApp) : base(next, optionsAccessor, uHubApiApp)
        { }

        public override AppNoticeResponseModel ExecuteApi(DeleteUserModel model)
        {
            return _uHubApiApp.DeleteUser(model);
        }
    }
}
