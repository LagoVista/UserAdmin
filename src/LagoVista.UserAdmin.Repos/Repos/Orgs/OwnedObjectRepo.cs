// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 968130b95f7f19a3519e17f93e32900c04faef5e61e3388d8b9b7a7b49669983
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.CloudStorage.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Orgs;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Orgs
{
    public class OwnedObjectRepo : DocumentDBRepoBase<OwnedObject>, IOwnedObjectRepo
    {
        public OwnedObjectRepo(IUserAdminSettings userAdminSettings, IDocumentCloudCachedServices services) :
                  base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, services)
        {
        }

        public Task<ListResponse<OwnedObject>> GetOwnedObjectsForOrgAsync(string orgid, ListRequest listRequest)
        {
            return QueryAllAsync(oo => oo.OwnerOrganization.Id == orgid, listRequest);
        }
    }
}
