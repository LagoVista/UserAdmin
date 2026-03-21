using LagoVista.Core.Interfaces;
using LagoVista.Core.Interfaces.AutoMapper;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Relational.DataContexts;
using LagoVista.Relational.Tests.Core.Utils;
using LagoVista.UserAdmin.Repos.Relational;
using Relational.Tests.Core.Utils;

namespace UserAdmin.RepoTest
{
    public class UserAdminRepos
    {
        private readonly BillingDataContext _billing; 
        private readonly MetricsDataContext _metricsDataContext;

        private readonly IAdminLogger _logger;
        private readonly ILagoVistaAutoMapper _autoMapper;
        private readonly ISecureStorage _secureStorage;
        private readonly ISystemUsers _systemUsers;
        private readonly IAppConfig _appConfig;

        public UserAdminRepos(
            BillingDataContext billing,
            MetricsDataContext metricsDataContext,
            IAdminLogger logger,
            ILagoVistaAutoMapper autoMapper,
            ISecureStorage secureStorage)
        {
            _billing = billing;
            _metricsDataContext = metricsDataContext;

            _logger = logger;
            _autoMapper = autoMapper;
            _secureStorage = secureStorage;
            _systemUsers = new RelationalTestSystemUsers();
            _appConfig = new TestAppConfig();
        }


        private AppUserRelationalRepo _appUsers;
        public AppUserRelationalRepo AppUsers => _appUsers ??= new AppUserRelationalRepo(_billing, _logger, _autoMapper, _secureStorage);

        private OrganizationRelationalRepo _org;
        public OrganizationRelationalRepo Organizations => _org ??= new OrganizationRelationalRepo(_billing, _logger, _autoMapper, _secureStorage);


        private SubscriptionRepo _subscriptionRepo;
        public SubscriptionRepo Subscriptions => _subscriptionRepo ??= new SubscriptionRepo(_billing, _logger, _autoMapper, _secureStorage);


        public ISystemUsers SystemUsers => _systemUsers;
    }
}
