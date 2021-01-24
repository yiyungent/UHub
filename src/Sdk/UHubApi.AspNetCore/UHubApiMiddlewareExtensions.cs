using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using UHubApi.AspNetCore.Middlewares;

namespace UHubApi.AspNetCore
{
    public static class UHubApiMiddlewareExtensions
    {
        public static IApplicationBuilder UseUHubApi(this IApplicationBuilder builder)
        {
            builder.Map("/api/UHub/RenameUser", app => app.UseMiddleware<RenameUserMiddleware>());
            builder.Map("/api/UHub/DeleteUser", app => app.UseMiddleware<DeleteUserMiddleware>());
            builder.Map("/api/UHub/AddUser", app => app.UseMiddleware<AddUserMiddleware>());

            return builder;
        }
    }
}
