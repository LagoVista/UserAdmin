using Relational.Tests.Core.Database;
using Relational.Tests.Core.Seeds;
using Relational.Tests.Core.Utils;
using System;
using System.Threading.Tasks;
using UserAdmin.RepoTest;

namespace UserAdmin.RepoTests
{
    public abstract class OrganizationRelationalRepoTests : UserAdminTestBase
    {
        [Test]
        public async Task Should_Add_And_Get_Organization()
        {
            await UserAdminRepos.AppUsers.AddAppUserAsync(UserSeeds.All[1], OrgEH, UserEH).ConfigureAwait(false);  
            await UserAdminRepos.Organizations.AddOrganizationAsync(OrganizationSeeds.Secondary, OrgEH, UserEH).ConfigureAwait(false);

            var fetched = await UserAdminRepos.Organizations.GetOrganizationAsync(OrganizationSeeds.Secondary.OrgId, OrgEH, UserEH).ConfigureAwait(false);
            fetched.BillingContact = null; // get will include billing contexct, it's not part of the original seed, so ignore for comparison
            Console.WriteLine(fetched.OrgName + " " + OrganizationSeeds.Secondary.OrgName + " " + fetched.OrgId + " " + OrganizationSeeds.Secondary.OrgId);

            var compare = SameTypePropertyComparer.Compare(OrganizationSeeds.Secondary, fetched);
            Assert.That(compare.Success, Is.True, compare.ToString());
        }

        [Test]
        public async Task Should_Update_Organization()
        {
            await UserAdminRepos.AppUsers.AddAppUserAsync(UserSeeds.All[1], OrgEH, UserEH).ConfigureAwait(false);
            await UserAdminRepos.Organizations.AddOrganizationAsync(OrganizationSeeds.Secondary, OrgEH, UserEH).ConfigureAwait(false);

            OrganizationSeeds.Secondary.OrgName = "Updated Org Name";
            OrganizationSeeds.Secondary.Status = "Active";
            OrganizationSeeds.Secondary.LastUpdatedDate = DateTime.UtcNow;

            await UserAdminRepos.Organizations.UpdateOrganizationAsync(OrganizationSeeds.Secondary, OrgEH, UserEH).ConfigureAwait(false);

            var fetched = await UserAdminRepos.Organizations.GetOrganizationAsync(OrganizationSeeds.Secondary.OrgId, OrgEH, UserEH).ConfigureAwait(false);
            fetched.BillingContact = null; // get will include billing contexct, it's not part of the original seed, so ignore for comparison

            var compare = SameTypePropertyComparer.Compare(OrganizationSeeds.Secondary, fetched);
            Assert.That(compare.Success, Is.True, compare.ToString());
        }

        [Test]
        public async Task Should_Delete_Organization()
        {
            await UserAdminRepos.AppUsers.AddAppUserAsync(UserSeeds.All[1], OrgEH, UserEH).ConfigureAwait(false);
            await UserAdminRepos.Organizations.AddOrganizationAsync(OrganizationSeeds.Secondary, OrgEH, UserEH).ConfigureAwait(false);

            var fetched = await UserAdminRepos.Organizations.GetOrganizationAsync(OrganizationSeeds.Secondary.OrgId, OrgEH, UserEH).ConfigureAwait(false);
            Assert.That(fetched, Is.Not.Null);

            await UserAdminRepos.Organizations.DeleteOrganizationAsync(OrganizationSeeds.Secondary.OrgId, OrgEH, UserEH).ConfigureAwait(false);

            var loadedOrg = await UserAdminRepos.Organizations.GetOrganizationAsync(OrganizationSeeds.Secondary.OrgId, OrgEH, UserEH).ConfigureAwait(false);
            Assert.That(loadedOrg, Is.Null);
        }
    }

    #region Provider Test Fixtures
#if TEST_PROVIDER_SQLITE
    [TestFixture]
    public sealed class OrganizationRelationalRepo_Sqlite : OrganizationRelationalRepoTests
    {
        protected override EfTestProvider Provider => EfTestProvider.SqliteInMemory;
    }
#endif

#if TEST_PROVIDER_SQL

    [TestFixture]
    public sealed class OrganizationRelationalRepo_SqlServer : OrganizationRelationalRepoTests
    {
        protected override EfTestProvider Provider => EfTestProvider.SqlServer;
    }
#endif

#if TEST_PROVIDER_POSTGRES

    [TestFixture]
    public sealed class OrganizationRelationalRepo_Postgres : OrganizationRelationalRepoTests
    {
        protected override EfTestProvider Provider => EfTestProvider.Postgres;
    }
#endif
    #endregion
}
