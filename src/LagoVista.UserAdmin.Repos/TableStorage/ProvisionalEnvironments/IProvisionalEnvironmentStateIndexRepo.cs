using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.ProvisionalEnvironments
{
    internal interface IProvisionalEnvironmentStateIndexRepo
    {
        Task InsertAsync(ProvisionalEnvironmentState state, DateTime expiresUtc, string environmentId);
        Task<bool> ExistsAsync(ProvisionalEnvironmentState state, DateTime expiresUtc, string environmentId);
        Task<IEnumerable<string>> FindEnvironmentIdsAsync(ProvisionalEnvironmentState state, DateTime? expiresBeforeUtc, int take);
        Task DeleteAsync(ProvisionalEnvironmentState state, DateTime expiresUtc, string environmentId);
    }
}
