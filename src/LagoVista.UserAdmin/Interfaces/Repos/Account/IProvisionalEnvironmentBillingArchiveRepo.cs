using LagoVista.UserAdmin.Models.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Users
{
    public interface IProvisionalEnvironmentBillingArchiveRepo
    {
        Task<IReadOnlyCollection<ProvisionalEnvironmentBillingEventArchive>> GetBillingEventsAsync(string organizationId, string subscriptionId);
        Task<int> DeleteBillingEventsAsync(string organizationId, string subscriptionId, IReadOnlyCollection<string> billingEventIds);
    }
}
