using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IDistributionManager
    {
        Task<InvokeResult> AddListAsync(DistroList list, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdatedListAsync(DistroList list, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteListAsync(string id, EntityHeader org, EntityHeader user);
        Task<DistroList> GetListAsync(string id, EntityHeader org, EntityHeader user);
        Task<ListResponse<DistroListSummary>> GetListsForOrgAsync(EntityHeader org, EntityHeader user, ListRequest listRequest);
        Task<bool> QueryKeyInUseAsync(string key, string orgId);
        Task<DependentObjectCheckResult> CheckInUse(string id, EntityHeader org, EntityHeader user);
        Task SendEmailNotification(string subject, string message, string distroListId, EntityHeader org, EntityHeader user);
        Task SendSmsNotification(string message, string distroListId, EntityHeader org, EntityHeader user);
    }
}
