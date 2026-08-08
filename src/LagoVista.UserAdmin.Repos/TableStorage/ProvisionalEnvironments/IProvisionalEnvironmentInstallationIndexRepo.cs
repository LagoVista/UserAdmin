using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.ProvisionalEnvironments
{
    internal interface IProvisionalEnvironmentInstallationIndexRepo
    {
        Task InsertAsync(string installationIdHash, string environmentId, DateTime createdUtc);
        Task<string> FindEnvironmentIdAsync(string installationIdHash);
        Task DeleteAsync(string installationIdHash);
    }
}
