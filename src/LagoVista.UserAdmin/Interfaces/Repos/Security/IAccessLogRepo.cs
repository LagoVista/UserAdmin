// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: deeb2634ef032b5522ffe5d58572037967833ccf6d207c0654bc4517f0466378
// IndexVersion: 0
// --- END CODE INDEX META ---
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
