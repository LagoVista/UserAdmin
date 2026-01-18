using LagoVista.CloudStorage.DocumentDB;
using LagoVista.CloudStorage.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Interfaces.Repos.Testing;
using LagoVista.UserAdmin.Models.Testing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Testing
{
    internal class AuthViewRepo : DocumentDBRepoBase<AuthView>, IAuthViewRepo
    {
        bool _shouldConsolidateCollections;
        public AuthViewRepo(IUserAdminSettings userAdminSettings, IDocumentCloudCachedServices services) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, services)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public async Task AddAuthViewAsync(AuthView dsl)
        {
            await CreateDocumentAsync(dsl);
        }

        public Task DeleteByIdAsync(string id)
        {
            return base.DeleteDocumentAsync(id);
        }

        public Task<AuthView> GetByIdAsync(string id)
        {
            return base.GetDocumentAsync(id);
        }

        public Task<ListResponse<AuthViewSummary>> ListAsync(string orgId, ListRequest request)
        {
            return QuerySummaryDescendingAsync<AuthViewSummary, AuthView>(qry => qry.OwnerOrganization.Id == orgId, qry => qry.Name, request);
        }

        public Task UpdateAuthViewAsync(AuthView dsl)
        {
            return UpsertDocumentAsync(dsl);
        }

    }
}
