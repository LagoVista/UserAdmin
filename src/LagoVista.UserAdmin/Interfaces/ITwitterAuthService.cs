using LagoVista.UserAdmin.Models.Security;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces
{
    public interface ITwitterAuthService
    {
        Task<TwitterRequestToken> ObtainRequestTokenAsync(CancellationToken? token = null);

        Task<TwitterAccessToken> ObtainAccessTokenAsync(TwitterRequestToken token, string verifier, CancellationToken? cancellationToken = null);
    }
}
