using LagoVista.UserAdmin.Models.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IAccessLogRepo
    {
        Task AddActivityAsync(AccessLog accessLog);
        void AddActivity(AccessLog accessLog);
        Task<IEnumerable<AccessLog>> GetForResourceAsync(string resourceId);

        Task<IEnumerable<AccessLog>> GetForResourceAsync(string resourceId, string startTimeStamp, string endTimeStamp);
    }
}
