// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c06e541d390d76baa1b5ff38e5b4d967e72f14f6e396574060c6b447c9aff9ea
// IndexVersion: 2
// --- END CODE INDEX META ---
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
