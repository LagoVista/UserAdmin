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
