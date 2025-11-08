// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a25ff02fc36d4a85553bb4b7e8574742c38bcd2caecd4ccc5792c408f2adf5aa
// IndexVersion: 2
// --- END CODE INDEX META ---
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
