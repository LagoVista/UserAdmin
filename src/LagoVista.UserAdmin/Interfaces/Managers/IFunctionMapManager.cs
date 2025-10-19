// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c966151e6e657b923f518ed4d22d4a27636e66530e1618a714b87bd3a87ca6e0
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Security;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IFunctionMapManager
    {
        Task<InvokeResult> AddFunctionMapAsync(FunctionMap map, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateFunctionMapAsync(FunctionMap map, EntityHeader org, EntityHeader user);
        Task<FunctionMap> GetFunctionMapAsync(string id, EntityHeader org, EntityHeader user);
        Task<FunctionMap> GetFunctionMapByKeyAsync(string key, EntityHeader org, EntityHeader user);
        Task<FunctionMap> GetTopLevelFunctionMapAsync(EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteFunctionMapAsync(string id, EntityHeader org, EntityHeader user);
    }
}
