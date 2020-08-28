using Microsoft.AspNetCore.Authorization;

namespace UHub.Web.Authorization
{
    public class AdminRequirement : IAuthorizationRequirement
    {
        public AdminRequirement()
        {

        }
    }
}