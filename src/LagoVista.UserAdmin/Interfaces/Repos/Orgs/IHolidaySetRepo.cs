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
