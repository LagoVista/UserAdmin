// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 68fa0273bd3d0c595c7c427fda1c1d2abb723a4ec5ee177c10062545173756f5
// IndexVersion: 2
// --- END CODE INDEX META ---
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
