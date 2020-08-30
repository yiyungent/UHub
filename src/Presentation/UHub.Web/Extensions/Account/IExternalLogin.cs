using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace UHub.Web.Extensions.Account
{
    public interface IExternalLogin
    {
        string Name { get; }

        AuthenticationBuilder Add(AuthenticationBuilder builder, string clientId, string clientSecret);
    }
}
