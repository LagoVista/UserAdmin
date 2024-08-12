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
        Task<InvokeResult<string>> SendAsync(Email email, EntityHeader org, EntityHeader user);

        Task<InvokeResult> SendAsync(string email, string subject, string message, EntityHeader org, EntityHeader user);
        Task<InvokeResult<string>> RegisterContactAsync(Contact contact, Company company, EntityHeader org, EntityHeader user);
        Task<InvokeResult<string>> RegisterContactAsync(Contact contact, EntityHeader org, EntityHeader user);
        Task<InvokeResult<string>> CreateEmailListAsync(string listName, string customField, string id, EntityHeader org, EntityHeader user);

        Task<InvokeResult> DeleteEmailListAsync(string listId, EntityHeader org, EntityHeader user);
        Task<InvokeResult> AddContactToListAsync(string listId, string contactId, EntityHeader org, EntityHeader user);

        Task<InvokeResult<string>> AddEmailDesignAsync(string name, string subject, string htmlContents, string plainTextContent, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteEmailDesignAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateEmailDesignAsync(string id, string name, string subject, string htmlContents, string plainTextContent, EntityHeader org, EntityHeader user);

        Task<InvokeResult<AppUser>> AddEmailSenderAsync(AppUser sender, EntityHeader org, EntityHeader user);
        Task<InvokeResult<AppUser>> UpdateEmailSenderAsync(AppUser sender, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteEmailSenderAsync(string id, EntityHeader org, EntityHeader user);

        Task<InvokeResult> RefreshSegementAsync(string id, EntityHeader org, EntityHeader user);

        Task<ListResponse<ContactList>> GetListsAsync(EntityHeader org, EntityHeader user);

        Task<InvokeResult<string>> StartImportJobAsync(string fieldMappings, Stream stream, EntityHeader org, EntityHeader user);

        Task<ListResponse<EntityHeader>> GetEmailSendersAsync(EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateListAsync(string sendGridListId, string name, EntityHeader org, EntityHeader user);
    }
}
