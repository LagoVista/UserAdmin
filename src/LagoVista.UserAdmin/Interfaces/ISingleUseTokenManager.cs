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
