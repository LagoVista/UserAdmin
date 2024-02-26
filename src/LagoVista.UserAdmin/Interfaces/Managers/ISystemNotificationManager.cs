using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Apps;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces
{
    public interface ISystemNotificationManager
    {
        Task<InvokeResult> AddSystemNotification(NewSystemNoficiation notification);
        Task<InvokeResult> UpdateSystemNotification(SystemNotification notification);
        Task<ListResponse<SystemNotification>> GetOrgNotificationsAsync(EntityHeader org, EntityHeader user, ListRequest listRequest);
        Task<InvokeResult> RemoveSystemNotificationAsync(string parititionKey, string rowKey);
    }
}
