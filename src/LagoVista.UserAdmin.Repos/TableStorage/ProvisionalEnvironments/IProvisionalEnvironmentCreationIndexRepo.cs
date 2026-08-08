using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.ProvisionalEnvironments
{
    internal interface IProvisionalEnvironmentCreationIndexRepo
    {
        Task InsertAsync(string creationRequestId, string environmentId, DateTime createdUtc);
        Task<string> FindEnvironmentIdAsync(string creationRequestId);
        Task DeleteAsync(string creationRequestId);
    }
}
