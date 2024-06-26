﻿using LagoVista.Core.Models;
using LagoVista.Core.Validation;
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
    }
}
