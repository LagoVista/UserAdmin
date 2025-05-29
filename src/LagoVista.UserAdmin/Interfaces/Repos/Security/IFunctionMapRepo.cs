using LagoVista.UserAdmin.Models.Security;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IFunctionMapRepo
    {
        Task AddFunctionMapAsync(FunctionMap FunctionMap);
        Task UpdateFunctionMapAsync(FunctionMap FunctionMap);
        Task DeleteFunctionMapAsync(string id);
        Task<FunctionMap> GetTopLevelFunctionMapAsync(string orgId);
        Task<FunctionMap> GetFunctionMapAsync(string id);
        Task<FunctionMap> GetFunctionMapByKeyAsync(string orgId, String key);
    }
}
