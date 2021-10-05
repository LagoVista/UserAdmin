using LagoVista.CloudStorage.DocumentDB;
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
