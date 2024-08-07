using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Contacts;
using LagoVista.UserAdmin.Models.Users;
using System.IO;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IEmailSender
    {
        Task<InvokeResult<string>> SendAsync(Email email);

        Task<InvokeResult> SendAsync(string email, string subject, string message);
        Task<InvokeResult<string>> RegisterContactAsync(Contact contact, Company company);
        Task<InvokeResult<string>> RegisterContactAsync(Contact contact, EntityHeader org);
        Task<InvokeResult<string>> CreateEmailListAsync(string listName, string orgId, string customField, string id);

        Task<InvokeResult> DeleteEmailListAsync(string orgId, string listId);
        Task<InvokeResult> AddContactToListAsync(string listId, string contactId);

        Task<InvokeResult<string>> AddEmailDesignAsync(string name, string subject, string htmlContents, string plainTextContent);
        Task<InvokeResult> DeleteEmailDesignAsync(string id);
        Task<InvokeResult> UpdateEmailDesignAsync(string id, string name, string subject, string htmlContents, string plainTextContent);

        Task<InvokeResult<AppUser>> AddEmailSenderAsync(AppUser sender);
        Task<InvokeResult<AppUser>> UpdateEmailSenderAsync(AppUser sender);
        Task<InvokeResult> DeleteEmailSenderAsync(string id);

        Task<InvokeResult> RefreshSegementAsync(string id);

        Task<ListResponse<ContactList>> GetListsAsync(string orgId);

        Task<InvokeResult<string>> StartImportJobAsync(string fieldMappings, Stream stream);

        Task<ListResponse<EntityHeader>> GetEmailSendersAsync(string orgId);
    }
}
