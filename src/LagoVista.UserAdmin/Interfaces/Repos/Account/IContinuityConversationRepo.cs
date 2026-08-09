using LagoVista.UserAdmin.Models.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Users
{
    public interface IContinuityConversationRepo
    {
        Task<IEnumerable<ContinuityConversationMessage>> GetAsync(string actorId);
        Task AppendAsync(string actorId, IEnumerable<ContinuityConversationMessage> messages);
        Task ClearAsync(string actorId);
    }
}
