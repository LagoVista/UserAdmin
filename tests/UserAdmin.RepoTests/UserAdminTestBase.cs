using BillingTests.Common.Seeds;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Interfaces.AutoMapper;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Relational.DataContexts;
using Relational.Tests.Core.Database;
using Relational.Tests.Core.Seeds;
using System.Threading.Tasks;

namespace UserAdmin.RepoTest
{

    public abstract class UserAdminTestBase : RepoTestBase
    {
        protected override void CreateRepos(
            BillingDataContext billing,
            MetricsDataContext metricsDataContext,
            IAdminLogger logger,
            ILagoVistaAutoMapper autoMapper,
            ISecureStorage secureStorage)
        {
            UserAdminRepos = new UserAdminRepos(billing, metricsDataContext, logger, autoMapper, secureStorage);
        }

        protected UserAdminRepos UserAdminRepos { get; private set; }

        protected async Task CreateDefaultOrgUser()
        {
            await UserAdminRepos.AppUsers.AddAppUserAsync(UserSeeds.Primary, OrgEH, UserEH).ConfigureAwait(false);
            await UserAdminRepos.Organizations.AddOrganizationAsync(OrganizationSeeds.Primary, OrgEH, UserEH).ConfigureAwait(false);
        }

        protected override async Task PopulateAsync()
        {
            OrganizationSeeds.Populate(10);
            UserSeeds.Populate(10);
            SubscriptionSeeds.Populate(10);
        }
    }
}
