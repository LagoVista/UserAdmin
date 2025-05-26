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
        Task<InvokeResult<FunctionMap>> GetFunctionMapAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult<FunctionMap>> GetFunctionMapByKeyAsync(string key, EntityHeader org, EntityHeader user);
        Task<InvokeResult<FunctionMap>> GetTopLevelFunctionMapAsync(EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteFunctionMapAsync(string id, EntityHeader org, EntityHeader user);
    }
}
