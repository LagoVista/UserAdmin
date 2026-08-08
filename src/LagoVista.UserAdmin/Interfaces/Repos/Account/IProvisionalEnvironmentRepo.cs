using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Users
{
    public interface IProvisionalEnvironmentRepo
    {
        Task CreateAsync(ProvisionalEnvironment environment);
        Task<ProvisionalEnvironment> GetByIdAsync(string id);
        Task<ProvisionalEnvironment> FindByCreationRequestIdAsync(string creationRequestId);
        Task<ProvisionalEnvironment> FindByRecoveryTokenHashAsync(string recoveryTokenHash);
        Task<ProvisionalEnvironment> FindByInstallationIdHashAsync(string installationIdHash);
        Task<IEnumerable<ProvisionalEnvironment>> GetByStateAsync(ProvisionalEnvironmentState state, DateTime? expiresBeforeUtc = null, int take = 100);
        Task UpdateAsync(ProvisionalEnvironment environment);
        Task DeleteAsync(string id);
    }
}
