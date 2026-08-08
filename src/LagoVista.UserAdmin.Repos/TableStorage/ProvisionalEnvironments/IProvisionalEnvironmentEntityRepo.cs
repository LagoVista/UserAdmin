using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.ProvisionalEnvironments
{
    internal interface IProvisionalEnvironmentEntityRepo
    {
        Task InsertAsync(ProvisionalEnvironment environment);
        Task<ProvisionalEnvironment> GetByIdAsync(string id);
        Task UpdateAsync(ProvisionalEnvironment environment);
        Task DeleteAsync(string id);
    }
}
