using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Users
{
    public interface IProvisionalEnvironmentArchiveStore
    {
        Task<ProvisionalEnvironmentArchiveWriteResult> WriteAndVerifyAsync(ProvisionalEnvironmentArchiveWriteRequest request);
    }
}
