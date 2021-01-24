using System;
using System.Collections.Generic;
using System.Text;
using UHubApi.AspNetCore.Models;

namespace UHubApi.AspNetCore
{
    public interface IUHubApiApp
    {
        AppNoticeResponseModel AddUser(AddUserModel model);

        AppNoticeResponseModel DeleteUser(DeleteUserModel model);

        AppNoticeResponseModel RenameUser(RenameUserModel model);
    }
}
