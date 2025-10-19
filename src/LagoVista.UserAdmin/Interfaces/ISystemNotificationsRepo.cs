// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7a70c8cb7378a83e8ddc289cf6643ec930ec2ab5861c7802cfc6a29694f340ca
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Apps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin
{
    public interface ISystemNotificationsRepo
    {
        Task AddSystemNotificationAsync(SystemNotification item);
        Task UpdateSystemNotificationAsync(SystemNotification item);
        Task DeleteSystemNotificationAsync(string partitionKey, string rowKey);
        Task<IEnumerable<SystemNotification>> GetSystemAndPublicNotifications(string orgId);
        Task<ListResponse<SystemNotification>> GetOrgNotifications(string orgId, ListRequest listRequest);
    }
}
