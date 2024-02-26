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
