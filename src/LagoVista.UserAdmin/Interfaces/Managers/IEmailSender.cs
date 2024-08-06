using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
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
        Task<InvokeResult<string>> CreateEmailListAsync(string listName, string customField, string id);
        Task<InvokeResult> AddContactToListAsync(string listId, string contactId);

        Task<InvokeResult<string>> AddEmailDesignAsync(string name, string subject, string htmlContents, string plainTextContent);
        Task<InvokeResult> DeleteEmailDesignAsync(string id);
        Task<InvokeResult> UpdateEmailDesignAsync(string id, string name, string subject, string htmlContents, string plainTextContent);

        Task<InvokeResult<AppUser>> AddEmailSenderAsync(AppUser sender);
        Task<InvokeResult<AppUser>> UpdateEmailSenderAsync(AppUser sender);
        Task<InvokeResult> DeleteEmailSenderAsync(string id);

        Task<InvokeResult<string>> StartImportJobAsync(string fieldMappings, Stream stream);
    }
}
