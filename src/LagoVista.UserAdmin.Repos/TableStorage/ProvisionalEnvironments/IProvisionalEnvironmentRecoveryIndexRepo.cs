using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.ProvisionalEnvironments
{
    internal interface IProvisionalEnvironmentRecoveryIndexRepo
    {
        Task InsertAsync(string recoveryTokenHash, string environmentId, DateTime createdUtc);
        Task<string> FindEnvironmentIdAsync(string recoveryTokenHash);
        Task DeleteAsync(string recoveryTokenHash);
    }
}
