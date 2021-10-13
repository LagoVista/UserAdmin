using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface  IRDBMSManager
    {
        Task<InvokeResult> AddAppUserAsync(AppUser user);
        Task<InvokeResult> DeleteAppUserAsync(string userId);
        Task<InvokeResult> DeleteOrgAsync(string orgId);
        Task<InvokeResult> UpdateAppUserAsync(AppUser user);
        Task<InvokeResult> AddOrgAsync(Models.Orgs.Organization org);
        Task<InvokeResult> UpdateOrgAsync(Models.Orgs.Organization org);
        Task<bool> HasBillingRecords(string orgId);
        Task<List<EntityHeader>> GetBillingContactOrgsForUserAsync(string userId);
    }
}
