// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 33f493e5cd7b80bd639be30bdddafc60faedf4e6f8e654401bf41599864c91ca
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin
{
    public interface IOwnedObjectRepo
    {
        Task<ListResponse<OwnedObject>> GetOwnedObjectsForOrgAsync(string orgid, ListRequest listRequest);
    }
}
