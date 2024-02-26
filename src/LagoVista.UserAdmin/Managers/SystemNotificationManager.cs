using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Models.Apps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class SystemNotificationManager : ISystemNotificationManager
    {
        private readonly ISystemNotificationsRepo _systemNotificationRepo;

        public SystemNotificationManager(ISystemNotificationsRepo systemNotificationRepo)
        {
            _systemNotificationRepo = systemNotificationRepo ?? throw new ArgumentNullException(nameof(systemNotificationRepo));
        }

        public async Task<InvokeResult> AddSystemNotification(NewSystemNoficiation notification)
        {
            await _systemNotificationRepo.AddSystemNotificationAsync(notification.ToSystemNotification());
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateSystemNotification(SystemNotification notification)
        {
            await _systemNotificationRepo.AddSystemNotificationAsync(notification);
            return InvokeResult.Success;
        }

        public Task<ListResponse<SystemNotification>> GetOrgNotificationsAsync(EntityHeader org, EntityHeader user, ListRequest listRequest)
        {
            return _systemNotificationRepo.GetOrgNotifications(org.Id, listRequest);
        }

        public async Task<InvokeResult> RemoveSystemNotificationAsync(string parititionKey, string rowKey)
        {
            await _systemNotificationRepo.DeleteSystemNotificationAsync(parititionKey, rowKey);
            return InvokeResult.Success;
        }
    }
}
