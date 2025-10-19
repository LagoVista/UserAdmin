// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0d6c6042af6104db8bf250a036003d63c0b308f048c5b91c0eb903b5271a6ba6
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin
{
    public interface ISingleUseTokenManager
    {
        Task<InvokeResult<SingleUseToken>> CreateAsync(string userId, TimeSpan? expires = null);
    
        Task<InvokeResult> ValidationAsync(string userId, string tokenId);
    }
}
