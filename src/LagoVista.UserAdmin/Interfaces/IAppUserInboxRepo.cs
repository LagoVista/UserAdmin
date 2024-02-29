using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Apps;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin
{
    public interface IAppUserInboxRepo
    {
        Task AddInboxItemAsync(AppUserInboxItem item);
        Task UpdateInboxItemAsync(AppUserInboxItem item);
        Task DeleteItemAsync(string partitionKey, string rowKey);
        Task<AppUserInboxItem> GetItemInboxItemAsync(string paritionKey, string itemId);     
        Task<ListResponse<AppUserInboxItem>> GetInboxItemsAsync(string orgId, string userId, ListRequest listRequest, bool unreadOnly = false);
    }
}
