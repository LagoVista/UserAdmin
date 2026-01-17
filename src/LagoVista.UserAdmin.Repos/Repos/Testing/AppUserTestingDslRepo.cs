
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.CloudStorage.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Testing;

namespace LagoVista.UserAdmin.Repos.Testing
{
    public class AppUserTestingDslRepo : DocumentDBRepoBase<AppUserTestingDSL>, IAppUserTestingDslRepo
    {
        bool _shouldConsolidateCollections;
        public AppUserTestingDslRepo(IUserAdminSettings userAdminSettings, IDocumentCloudCachedServices services) : 
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, services)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public async Task AddDSLAsync(AppUserTestingDSL dsl)
        {
            await CreateDocumentAsync(dsl);
        }

        public Task DeleteByIdAsync(string id)
        {
            return base.DeleteDocumentAsync(id);
        }

        public Task<AppUserTestingDSL> GetByIdAsync(string id)
        {
            return base.GetDocumentAsync(id);
        }

        public Task<ListResponse<AppUserTestingDSLSummary>> ListAsync(string orgId, ListRequest request)
        {
             return QuerySummaryDescendingAsync< AppUserTestingDSLSummary, AppUserTestingDSL>(qry => qry.OwnerOrganization.Id == orgId, qry=>qry.Name, request);
        }

        public Task UpdateDSLAsync(AppUserTestingDSL dsl)
        {      
            return UpsertDocumentAsync(dsl); 
        }

    }
}