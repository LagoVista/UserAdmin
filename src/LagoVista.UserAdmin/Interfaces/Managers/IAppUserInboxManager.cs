// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ac4aab69390c345f6843de8ccf37693e7b35a0d0a1c36f306ae43ebdebfe72c6
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Repos.Repos.Account;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces
{
    public interface IAppUserInboxManager
    {
        Task<InvokeResult> CreateInboxItemAsync(NewAppUserInboxItemItem item);
        Task<InvokeResult> MarkAsReadAsync(string partitionKey, string rowKey, EntityHeader org, EntityHeader user);
        Task<InvokeResult<int>> GetAllInboxItemCountAsync(EntityHeader org, EntityHeader user);
        Task<ListResponse<InboxItem>> GetAllInboxItemsAsync(EntityHeader org, EntityHeader user, ListRequest listRequest);
        Task<ListResponse<AppUserInboxItem>> GetUnreadInboxItemsAsync(EntityHeader org, EntityHeader user, ListRequest listRequest);
        Task<InvokeResult> DeleteItemAsync(string partitionKey, string rowKey, EntityHeader org, EntityHeader user);
    }
}
