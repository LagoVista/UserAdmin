using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Security
{
    public class AccessLogRepo : TableStorageBase<AccessLog>, IAccessLogRepo
    {
        public AccessLogRepo(IUserAdminSettings settings, IAdminLogger logger) :
            base(settings.AccessLogTableStorage.AccountId, settings.AccessLogTableStorage.AccessKey, logger)
        {

        }

        public void AddActivity(AccessLog accessLog)
        {
            Task.Run(async () =>  {
                await AddActivityAsync(accessLog);
            });
        }

        public Task AddActivityAsync(AccessLog accessLog)
        {
            return InsertAsync(accessLog);
        }

        public Task<IEnumerable<AccessLog>> GetForResourceAsync(string resourceId)
        {
            return GetByParitionIdAsync(resourceId);
        }

        public Task<IEnumerable<AccessLog>> GetForResourceAsync(string resourceId, string startTimeStamp, string endTimeStamp)
        {
            throw new NotImplementedException();
        }
    }
}
