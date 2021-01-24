using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UHubApi.AspNetCore.Models;

namespace UHubApi.AspNetCore.Middlewares
{
    public class AddUserMiddleware : UHubApiMiddleware<AddUserModel>
    {
        public AddUserMiddleware(RequestDelegate next, IOptions<UHubApiOptions> optionsAccessor, IUHubApiApp uHubApiApp) : base(next, optionsAccessor, uHubApiApp)
        { }

        public override AppNoticeResponseModel ExecuteApi(AddUserModel model)
        {
            return _uHubApiApp.AddUser(model);
        }
    }
}
