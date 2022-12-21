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
