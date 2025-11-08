// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 31cb49d45f7a4f723374b6f6a7dc2034eccc03ca51b6ff08d67cd53adb71e597
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using System.Collections.Generic;
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
        Task<ListResponse<DistroListSummary>> GetListsForCustomerAsync(string customerId, EntityHeader org, EntityHeader user, ListRequest listRequest);
        Task<bool> QueryKeyInUseAsync(string key, string orgId);
        Task<DependentObjectCheckResult> CheckInUse(string id, EntityHeader org, EntityHeader user);
        Task SendEmailNotification(string subject, string message, string distroListId, EntityHeader org, EntityHeader user);
        Task SendSmsNotification(string message, string distroListId, EntityHeader org, EntityHeader user);
        Task<InvokeResult<string>> ConfirmAppUserAsync(string distroListId, string appUserId, string contactMethod);
        Task<InvokeResult<string>> ConfirmExternalContact(string distroListId, string externalContactId, string contactMethod);
        Task<InvokeResult> SendTestAsync(string id, EntityHeader org, EntityHeader user);
        Task<List<NotificationContact>> GetAllContactsAsync(EntityHeader distributionList, List<NotificationContact> contacts = null);
        Task<List<NotificationContact>> GetAllContactsAsync(string distributionListId, List<NotificationContact> contacts = null);
        Task<List<NotificationContact>> AddContactAsync(EntityHeader user, List<NotificationContact> contacts = null);
    }
}
