using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Testing;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Testing
{
    public interface IAuthViewRepo
    {
        Task<AuthView> GetByIdAsync(string id);
        Task<ListResponse<AuthViewSummary>> ListAsync(string orgId, ListRequest request);

        Task AddAuthViewAsync(AuthView dsl);

        Task UpdateAuthViewAsync(AuthView dsl);

        Task DeleteByIdAsync(string id);

    }
}
