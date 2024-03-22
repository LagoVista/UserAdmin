using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IEmailSender
    {
        Task<InvokeResult> SendAsync(string email, string subject, string message);
        Task<InvokeResult<string>> RegisterContactAsync(Contact contact, Company company);
        Task<InvokeResult<string>> RegisterContactAsync(Contact contact, EntityHeader org);
        Task<InvokeResult<string>> CreateEmailListAsync(string listName);
        Task<InvokeResult> AddContactToListAsync(string listId, string contactId);
    }
}
