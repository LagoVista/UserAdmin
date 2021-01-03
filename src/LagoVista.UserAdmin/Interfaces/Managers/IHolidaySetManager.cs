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
        Task<InvokeResult> DeleteHolidaySetAsync(String holidaySetId, EntityHeader org, EntityHeader user);
        Task<ListResponse<HolidaySetSummary>> GetAllHolidaySets(ListRequest listRequest, EntityHeader org, EntityHeader user );
        Task<bool> QueryKeyInUseAsync(string key, EntityHeader org);
        Task<DependentObjectCheckResult> CheckHolidaySetInUseAsync(String holidaySetId, EntityHeader org, EntityHeader user);
        Task<InvokeResult> CopyToOrgAsync(string holidaySetId, EntityHeader org, EntityHeader user);
    }
}
