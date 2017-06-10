using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.PlatformSupport;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.IoT.Logging.Loggers;

namespace LagoVista.UserAdmin.Repos.Repos.Orgs
{
    public class SubscriptionRepo : DocumentDBRepoBase<Subscription>, ISubscriptionRepo
    {
        bool _shouldConsolidateCollections;

        public SubscriptionRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger) : base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddSubscriptionAsync(Subscription subscription)
        {
            return CreateDocumentAsync(subscription);
        }

        public Task DeleteSubscriptionAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<Subscription> GetSubscriptionAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public async Task<IEnumerable<SubscriptionSummary>> GetSubscriptionsForOrgAsync(string orgId)
        {
            var items = await base.QueryAsync(qry => qry.IsPublic == true || qry.OwnerOrganization.Id == orgId);

            return from item in items
                   select item.CreateSummary();
        }

        public async Task<bool> QueryKeyInUse(string key, string orgId)
        {
            var items = await base.QueryAsync(attr => (attr.OwnerOrganization.Id == orgId || attr.IsPublic == true) && attr.Key == key);
            return items.Any();
        }

        public Task UpdateSubscriptionAsync(Subscription subscription)
        {
            return UpsertDocumentAsync(subscription);
        }
    }
}
