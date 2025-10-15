using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Contacts;
using LagoVista.UserAdmin.Models.Users;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IEmailSender
    {
        Task<InvokeResult> SendToAppUserAsync(string appuUserId, string subject, string body);
        Task<InvokeResult> SendAsync(string email, string subject, string body, bool hasFullEmail = false, string appName = "", string appLogo = "");

        Task<InvokeResult<string>> SendAsync(Email email, EntityHeader org, EntityHeader user);

        Task<InvokeResult> SendInBackgroundAsync(Email email, EntityHeader org, EntityHeader user);

        Task<InvokeResult> SendAsync(string email, string subject, string message, EntityHeader org, EntityHeader user, string appName = "", string appLogo = "");
        Task<InvokeResult> SendInBackgroundAsync(string email, string subject, string body, EntityHeader org, EntityHeader user);
        Task<InvokeResult<string>> RegisterContactAsync(Contact contact, Company company, EntityHeader org, EntityHeader user);
        Task<InvokeResult<string>> RegisterContactAsync(Contact contact, EntityHeader org, EntityHeader user);
        Task<InvokeResult<string>> CreateEmailListAsync(string listName, string customField, string id, EntityHeader org, EntityHeader user);

        Task<InvokeResult> DeleteEmailListAsync(string listId, EntityHeader org, EntityHeader user);
        Task<InvokeResult> AddContactToListAsync(string listId, string contactId, EntityHeader org, EntityHeader user);

        Task<InvokeResult<string>> AddEmailDesignAsync(string name, string subject, string htmlContents, string plainTextContent, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteEmailDesignAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateEmailDesignAsync(string id, string name, string subject, string htmlContents, string plainTextContent, EntityHeader org, EntityHeader user);
        Task<ListResponse<EmailDesign>> GetEmailDesignsAsync(EntityHeader org, EntityHeader user);

        Task<InvokeResult<AppUser>> AddEmailSenderAsync(AppUser sender, string nickNameOverride, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteEmailSenderAsync(string id, EntityHeader org, EntityHeader user);
        Task<ListResponse<EmailSenderSummary>> GetEmailSendersAsync(EntityHeader org, EntityHeader user);
        Task<EmailSender> GetEmailSenderAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> AddEmailSenderAsync(EmailSender sender, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateEmailSenderAsync(EmailSender sender, EntityHeader org, EntityHeader user);
        Task<InvokeResult> SendEmailSenderVerificationAsync(string senderId);


        Task<InvokeResult> RefreshSegementAsync(string id, EntityHeader org, EntityHeader user);

        Task<ListResponse<ContactList>> GetListsAsync(EntityHeader org, EntityHeader user);
        Task<InvokeResult<ContactList>> GetListAsync(string listId, EntityHeader org, EntityHeader user);

        Task<InvokeResult<string>> StartImportJobAsync(string fieldMappings, Stream stream, EntityHeader org, EntityHeader user);


        Task<InvokeResult> UpdateListAsync(string sendGridListId, string name, EntityHeader org, EntityHeader user);
        Task<InvokeResult<string>> SendToListAsync(string name, string listId, string senderId, string designId, EntityHeader org, EntityHeader user);

        Task<InvokeResult<string>> SendEmailSendListNowAsync(string singleSendId, EntityHeader org, EntityHeader user);
        Task<InvokeResult<string>> ScheduleEmailSendListAsync(string singleSendId, string scheduleDate, EntityHeader org, EntityHeader user);
        Task<ListResponse<EmailListSend>> GetEmailListSendsAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteEmailListSendAsync(string listId, EntityHeader org, EntityHeader user);
        Task<InvokeResult<EmailImportStatus>> GetImportJobStatusAsync(string jobId, EntityHeader org, EntityHeader user);
        List<string> GetRequiredImportFields();

        bool IsValidEmail(string email);
    }
}
