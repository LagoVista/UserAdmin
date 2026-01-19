
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.CloudStorage.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Testing;

namespace LagoVista.UserAdmin.Repos.Testing
{
    public class AppUserTestingDslRepo : DocumentDBRepoBase<AppUserTestScenario>, IAppUserTestingDslRepo
    {
        bool _shouldConsolidateCollections;
        public AppUserTestingDslRepo(IUserAdminSettings userAdminSettings, IAdminLogger adminLogger) : 
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, adminLogger)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public async Task AddDSLAsync(AppUserTestScenario dsl)
        {
            await CreateDocumentAsync(dsl);
        }

        public Task DeleteByIdAsync(string id)
        {
            return base.DeleteDocumentAsync(id);
        }

        public Task<AppUserTestScenario> GetByIdAsync(string id)
        {
            return base.GetDocumentAsync(id);
        }

        public Task<ListResponse<AppUserTestScenarioSummary>> ListAsync(string orgId, ListRequest request)
        {
             return QuerySummaryDescendingAsync<AppUserTestScenarioSummary, AppUserTestScenario>(qry => qry.OwnerOrganization.Id == orgId, qry=>qry.Name, request);
        }

        public Task UpdateTestScenarioAsync(AppUserTestScenario dsl)
        {      
            return UpsertDocumentAsync(dsl); 
        }

    }
}