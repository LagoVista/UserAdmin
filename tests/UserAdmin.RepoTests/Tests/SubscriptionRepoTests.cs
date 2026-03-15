using BillingTests.Common.Seeds;
using LagoVista.Core;
using Relational.Tests.Core.Database;
using Relational.Tests.Core.Seeds;
using Relational.Tests.Core.Utils;
using System.Threading.Tasks;
using UserAdmin.RepoTest;

namespace UserAdmin.RepoTests
{
    public abstract class SubscriptionRepoTests : UserAdminTestBase
    {
        [SetUp]
        public async Task Setup()
        {
            await UserAdminRepos.AppUsers.AddAppUserAsync(UserSeeds.Primary, OrgEH, UserEH).ConfigureAwait(false);
            await UserAdminRepos.Organizations.AddOrganizationAsync(OrganizationSeeds.Primary, OrgEH, UserEH).ConfigureAwait(false);
        }

        [Test]
        public async Task Should_Add_And_Get_Subscription()
        {
            await UserAdminRepos.Subscriptions.AddSubscriptionAsync(SubscriptionSeeds.Primary, OrgEH, UserEH).ConfigureAwait(false);
            var fetched = await UserAdminRepos.Subscriptions.GetSubscriptionAsync(SubscriptionSeeds.Primary.Id, OrgEH, UserEH).ConfigureAwait(false);

            var compare = SameTypePropertyComparer.Compare(SubscriptionSeeds.Primary, fetched);
            Assert.That(compare.Success, Is.True, compare.ToString());
        }

        [Test]
        public async Task Should_Update_Subscription()
        {
            await UserAdminRepos.Subscriptions.AddSubscriptionAsync(SubscriptionSeeds.Primary, OrgEH, UserEH).ConfigureAwait(false);

            SubscriptionSeeds.Primary.Name = "Updated Name";
            SubscriptionSeeds.Primary.End = CalendarDate.Today();

            await UserAdminRepos.Subscriptions.UpdateSubscriptionAsync(SubscriptionSeeds.Primary, OrgEH, UserEH).ConfigureAwait(false);

            var fetched = await UserAdminRepos.Subscriptions.GetSubscriptionAsync(SubscriptionSeeds.Primary.Id, OrgEH, UserEH).ConfigureAwait(false);

            var compare = SameTypePropertyComparer.Compare(SubscriptionSeeds.Primary, fetched);
            Assert.That(compare.Success, Is.True, compare.ToString());
        }

        [Test]
        public async Task Should_Update_Subscription_With_Crypto()
        {
            await UserAdminRepos.Subscriptions.AddSubscriptionAsync(SubscriptionSeeds.Primary, OrgEH, UserEH).ConfigureAwait(false);

            SubscriptionSeeds.Primary.Name = "Updated Name";
            SubscriptionSeeds.Primary.PaymentAccountType = "stripe";
            SubscriptionSeeds.Primary.PaymentAccountId = "abc1234";
            SubscriptionSeeds.Primary.End = CalendarDate.Today();

            await UserAdminRepos.Subscriptions.UpdateSubscriptionAsync(SubscriptionSeeds.Primary, OrgEH, UserEH).ConfigureAwait(false);

            var fetched = await UserAdminRepos.Subscriptions.GetSubscriptionAsync(SubscriptionSeeds.Primary.Id, OrgEH, UserEH).ConfigureAwait(false);

            var compare = SameTypePropertyComparer.Compare(SubscriptionSeeds.Primary, fetched);
            Assert.That(compare.Success, Is.True, compare.ToString());
        }
    }

    #region Provider Test Fixtures
#if TEST_PROVIDER_SQLITE
    [TestFixture]
    public sealed class SubscriptionRepoTests_Sqlite : SubscriptionRepoTests
    {
        protected override EfTestProvider Provider => EfTestProvider.SqliteInMemory;
    }
#endif

#if TEST_PROVIDER_SQL
    [TestFixture]
    public sealed class SubscriptionRepoTests_SqlServer : SubscriptionRepoTests
    {
        protected override EfTestProvider Provider => EfTestProvider.SqlServer;
    }
#endif

#if TEST_PROVIDER_POSTGRES
    [TestFixture, Category("Provider:Postgres")]
    public sealed class SubscriptionRepoTests_Postgres : SubscriptionRepoTests
    {
        protected override EfTestProvider Provider => EfTestProvider.Postgres;
    }
#endif
    #endregion
}
