using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.AnonymousVisitors
{
    internal interface IAnonymousVisitorEntityRepo
    {
        Task InsertAsync(AnonymousVisitor visitor);
        Task<AnonymousVisitor> GetByActorIdAsync(string actorId);
        Task UpdateAsync(AnonymousVisitor visitor);
        Task DeleteAsync(string actorId);
    }
}
