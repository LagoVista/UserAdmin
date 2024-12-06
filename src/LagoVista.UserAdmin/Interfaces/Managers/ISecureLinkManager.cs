using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface ISecureLinkManager
    {
        Task<InvokeResult<String>> GenerateSecureLinkAsync(string link, EntityHeader forUser, TimeSpan duration, EntityHeader org, EntityHeader user);
        Task<InvokeResult<string>> GetSecureLinkAsync(string orgId, string linkId);
    }
}
