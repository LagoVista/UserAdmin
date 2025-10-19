// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 968130b95f7f19a3519e17f93e32900c04faef5e61e3388d8b9b7a7b49669983
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Orgs
{
    public class OwnedObjectRepo : DocumentDBRepoBase<OwnedObject>, IOwnedObjectRepo
    {
        private readonly bool _shouldConsolidateCollections;

        public OwnedObjectRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger) :
                  base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;


        public Task<ListResponse<OwnedObject>> GetOwnedObjectsForOrgAsync(string orgid, ListRequest listRequest)
        {
            return QueryAllAsync(oo => oo.OwnerOrganization.Id == orgid, listRequest);
        }
    }
}
