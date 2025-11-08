// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b3f4028d48dfd03b80ce3f292f55277c58cac42f8e3d96686be80aac3ba8c146
// IndexVersion: 2
// --- END CODE INDEX META ---
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
