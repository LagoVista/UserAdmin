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
