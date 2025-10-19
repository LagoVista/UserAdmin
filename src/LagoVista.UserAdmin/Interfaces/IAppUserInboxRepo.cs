// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7ad76ca3ec77801a16b431d41d459297dc0aebcd59dccd12a1feaf66f90a4599
// IndexVersion: 0
// --- END CODE INDEX META ---
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
