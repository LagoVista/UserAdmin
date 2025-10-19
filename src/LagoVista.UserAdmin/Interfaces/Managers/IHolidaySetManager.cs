// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 3fc24858e4eb08ac98fe959b7d84a96dc678edac127443ca02f465a9b7733110
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IHolidaySetManager
    {
        Task<InvokeResult> AddHolidaySetAsync(HolidaySet holidaySet, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateHolidaySetAsync(HolidaySet holidaySet, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteHolidaySetAsync(string holidaySetId, EntityHeader org, EntityHeader user);
        Task<HolidaySet> GetHolidaySetAsync(string holidaySetId, EntityHeader org, EntityHeader user);
        Task<ListResponse<HolidaySetSummary>> GetAllHolidaySets(EntityHeader org, EntityHeader user, ListRequest listRequest);
        Task<bool> QueryKeyInUseAsync(string key, EntityHeader org);
        Task<DependentObjectCheckResult> CheckHolidaySetInUseAsync(String holidaySetId, EntityHeader org, EntityHeader user);
        Task<InvokeResult> CopyToOrgAsync(string holidaySetId, string destOrgId, EntityHeader org, EntityHeader user);
    }
}
