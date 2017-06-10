using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.PlatformSupport;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LagoVista.IoT.Logging.Loggers;

namespace LagoVista.UserAdmin.Repos.Repos.Account
{
    public class TeamRepo : DocumentDBRepoBase<Team>, ITeamRepo
    {
        bool _shouldConsolidateCollections;
        public TeamRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger) : 
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddTeamAsync(Team team)
        {
            return CreateDocumentAsync(team);
        }

        public Task DeleteTeamAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<Team> GetTeamAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public async Task<IEnumerable<TeamSummary>> GetTeamsForOrgAsync(string orgId)
        {
            var items = await base.QueryAsync(qry => qry.OwnerOrganization.Id == orgId);

            return from item in items
                   select item.CreateSummary();
        }

        public async Task<bool> QueryKeyInUseAsync(string key, string orgId)
        {
            var items = await base.QueryAsync(attr => (attr.OwnerOrganization.Id == orgId || attr.IsPublic == true) && attr.Key == key);
            return items.Any();
        }

        public Task UpdateTeamAsync(Team team)
        {
            return UpsertDocumentAsync(team);
        }
    }
}
