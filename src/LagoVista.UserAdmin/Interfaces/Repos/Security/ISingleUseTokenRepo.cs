// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 99ce8e86c133d7323b8b7ba8e3f0f2cdcfb26c3056b3860b955e6f93ab497ded
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin
{
    public interface ISingleUseTokenRepo
    {
        Task StoreAsync(SingleUseToken token);

        Task<SingleUseToken> RetreiveAsync(string userId, string tokenId);
    }
}
