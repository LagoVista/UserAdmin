using LagoVista.Models;
using Relational.Tests.Core.Database;
using Relational.Tests.Core.Seeds;
using Relational.Tests.Core.Utils;
using System;
using System.Threading.Tasks;
using UserAdmin.RepoTest;

namespace UserAdmin.RepoTests
{
    public abstract class AppUserRelationalRepoTests : UserAdminTestBase
    {
        [Test]
        public async Task Should_Add_And_Get_AppUser()
        {
            var user = UserSeeds.Secondary;

            await UserAdminRepos.AppUsers.AddAppUserAsync(user, OrgEH, UserEH).ConfigureAwait(false);
            var fetched = await UserAdminRepos.AppUsers.GetAppUserAsync(user.AppUserId, OrgEH, UserEH).ConfigureAwait(false);

            Assert.That(fetched, Is.Not.Null);

            var compare = SameTypePropertyComparer.Compare(user, fetched);
            Assert.That(compare.Success, Is.True, compare.ToString());
        }

        [Test]
        public async Task Should_Update_AppUser()
        {
            var user = UserSeeds.Secondary;

            await UserAdminRepos.AppUsers.AddAppUserAsync(user, OrgEH, UserEH).ConfigureAwait(false);

            user.FullName = "Updated Name";
            user.Email = "updated@test.local";
            user.LastUpdatedDate = DateTime.UtcNow;

            await UserAdminRepos.AppUsers.UpdateAppUserAsync(user, OrgEH, UserEH).ConfigureAwait(false);


            var fetched = await UserAdminRepos.AppUsers.GetAppUserAsync(user.AppUserId, OrgEH, UserEH).ConfigureAwait(false);
            var compare = SameTypePropertyComparer.Compare(user, fetched);
            Assert.That(compare.Success, Is.True, compare.ToString());
        }

        [Test]
        public async Task Should_Delete_AppUser()
        {
            var user = UserSeeds.Secondary;

            await UserAdminRepos.AppUsers.AddAppUserAsync(user, OrgEH, UserEH).ConfigureAwait(false);

            var fetched = await UserAdminRepos.AppUsers.GetAppUserAsync(user.AppUserId, OrgEH, UserEH).ConfigureAwait(false);
            Assert.That(fetched, Is.Not.Null);

            await UserAdminRepos.AppUsers.DeleteAppUserAsync(user.AppUserId, OrgEH, UserEH).ConfigureAwait(false);

            var loadedUser = await UserAdminRepos.AppUsers.GetAppUserAsync(user.AppUserId, OrgEH, UserEH).ConfigureAwait(false);
            Assert.That(loadedUser, Is.Null);
        }
    }

    #region Provider Test Fixtures
#if TEST_PROVIDER_SQLITE
    [TestFixture]
    public sealed class AppUserRelationalRepo_Sqlite : AppUserRelationalRepoTests
    {
        protected override EfTestProvider Provider => EfTestProvider.SqliteInMemory;
    }
#endif

#if TEST_PROVIDER_SQL
    [TestFixture]
    public sealed class AppUserRelationalRepo_SqlServer : AppUserRelationalRepoTests
    {
        protected override EfTestProvider Provider => EfTestProvider.SqlServer;
    }
#endif

#if TEST_PROVIDER_POSTGRES
    [TestFixture, Category("Provider:Postgres")]
    public sealed class AppUserRelationalRepo_Postgres : AppUserRelationalRepoTests
    {
        protected override EfTestProvider Provider => EfTestProvider.Postgres;
    }
#endif
    #endregion
}
