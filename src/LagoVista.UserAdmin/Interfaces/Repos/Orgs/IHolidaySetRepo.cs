// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 1837c4832974722444e16bfc6f2754dc685dbc121292e024937608ec35ab7b4f
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface IHolidaySetRepo
    {
        Task AddHolidaySetAsync(HolidaySet holidaySet);
        Task UpdateHolidaySetAsync(HolidaySet holidaySet);
        Task DeleteHolidaySetAsync(string id);

        Task<HolidaySet> GetHolidaySetAsync(String id);

        Task<ListResponse<HolidaySetSummary>> GetAllHolidaySetsAsync(string orgId, ListRequest listRequest);

        Task<bool> QueryKeyInUseAsync(string key, string orgId);
    }
}
