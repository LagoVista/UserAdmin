using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IAnonymousVisitorPromotionManager
    {
        Task<InvokeResult<AnonymousVisitorPromotionResponse>> PromoteAsync(string actorId, string ipAddress, AnonymousVisitorPromotionRequest request);
    }
}
