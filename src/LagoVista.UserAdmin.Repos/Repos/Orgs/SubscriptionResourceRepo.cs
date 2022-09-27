using LagoVista.CloudStorage;
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Orgs
{
    public class SubscriptionResourceRepo : DocumentDBRepoBase<SubscriptionResource>, ISubscriptionResourceRepo
    {
        bool _shouldConsolidateCollections;
        private readonly IAdminLogger _logger;

        public SubscriptionResourceRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger, ICacheProvider cacheProvider = null) :
                base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger, cacheProvider)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        protected override bool ShouldConsolidateCollections
        {
            get { return _shouldConsolidateCollections; }
        }

        public async Task<ListResponse<SubscriptionResource>> GetResourcesForSubscriptionAsync(Guid subscriptionId, ListRequest listRequest, string orgId)
        {
            return await QueryAsync(sub => sub.Id == subscriptionId.ToString(), listRequest);
        }
    }
}
