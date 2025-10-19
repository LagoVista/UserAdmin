// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b316e202d83bb892f866343d35717319381c77f04b85536d9df7951809303173
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.Logging.Utils;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Repos.RDBMS;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Tests.User
{
    [TestClass]
    public class UserOrgRDBMSTest : TestBase
    {
        private IRDBMSManager _dbUserManager;
        Mock<IRDBMSConnectionProvider> _cp = new Mock<IRDBMSConnectionProvider>();
        IAdminLogger _adminLogger = new AdminLogger(new ConsoleLogWriter());

        UserAdminDataContext _devShared;
        UserAdminDataContext _npDevDedicated;

        [TestInitialize]
        public void Init()
        {
            var devSQL = CloudStorage.Utils.TestConnections.DevSQLServer;

            var ssConnStr = $"Server={devSQL.Uri};Database={devSQL.ResourceName}; User Id={devSQL.UserName}; Password={devSQL.Password}";

            var sharedOptionsBuilder = new DbContextOptionsBuilder<UserAdminDataContext>()
                .UseSqlServer(ssConnStr);

            _devShared = new UserAdminDataContext(sharedOptionsBuilder.Options);

            var connstr = Environment.GetEnvironmentVariable("DEV_ORG_DB_CONNSTRING", EnvironmentVariableTarget.User);
            var dedicatedOptiosnBuilder = new DbContextOptionsBuilder<UserAdminDataContext>()
                .UseNpgsql(connstr);

            _npDevDedicated = new UserAdminDataContext(dedicatedOptiosnBuilder.Options);

            _dbUserManager = new RDBMSManager(_devShared, _cp.Object, _adminLogger);
            
            _cp.Setup(cp => cp.GetConnectionAsync(It.IsAny<string>())).ReturnsAsync(new RDBMSConnection(RDBMSConnectionType.PosgreSQLDedicated, connstr) { VerboseLogging = true });
            _cp.Setup(cp => cp.GetConnectionAsync(It.Is<string>(s => s == TEST_ORG_ID2))).ReturnsAsync(new RDBMSConnection(RDBMSConnectionType.SQLServerShared));
            _cp.Setup(cp => cp.GetConnectionAsync(It.Is<string>(s => s == TEST_ORG_ID3))).ReturnsAsync(new RDBMSConnection(RDBMSConnectionType.SQLServerShared));
        }


        [TestMethod]
        public async Task AddUserToPostgresAsync()
        {
            var appUser = new AppUser()
            {
                Id = TEST_USER_ID1,
                FirstName = "Test",
                LastName = "User",
                CreationDate = DateTime.UtcNow.ToJSONString(),
                LastUpdatedDate = DateTime.UtcNow.ToJSONString(),
                Email = "testuser@mydomain.com"
            };

            Assert.IsFalse(await _dbUserManager.UserExistsAsync(TEST_ORG_ID1, TEST_USER_ID1), "user should not exist");
            AssertValid(await _dbUserManager.AddAppUserToOrgAsyncAsync(TEST_ORG_ID1, appUser));
            Assert.IsTrue(await _dbUserManager.UserExistsAsync(TEST_ORG_ID1, TEST_USER_ID1), "user did not get created");

            Assert.IsTrue(await _npDevDedicated.AppUser.AnyAsync(usr => usr.AppUserId == TEST_USER_ID1));
        }

        [TestMethod]
        public async Task AddUserToSQLServerAsync()
        {
            var appUser = new AppUser()
            {
                Id = TEST_USER_ID2,
                FirstName = "Test",
                LastName = "User",
                CreationDate = DateTime.UtcNow.ToJSONString(),
                LastUpdatedDate = DateTime.UtcNow.ToJSONString(),
                Email = "testuser@mydomain.com"
            };

            Assert.IsFalse(await _dbUserManager.UserExistsAsync(TEST_ORG_ID2, TEST_USER_ID2), "user should not exist");
            AssertValid(await _dbUserManager.AddAppUserToOrgAsyncAsync(TEST_ORG_ID2, appUser));            
            Assert.IsTrue(await _dbUserManager.UserExistsAsync(TEST_ORG_ID2, TEST_USER_ID2), "user did not get created");

            // Confirm the record is in the dev shared sql server, not in postress
            Assert.IsTrue(await _devShared.AppUser.AnyAsync(usr => usr.AppUserId == TEST_USER_ID2), "user does not exist in SQL Server.");
        }

        [TestMethod]
        public async Task AddUserTwiceOnlyOneShouldExistsAsync()
        {
            var appUser = new AppUser()
            {
                Id = TEST_USER_ID2,
                FirstName = "Test",
                LastName = "User",
                CreationDate = DateTime.UtcNow.ToJSONString(),
                LastUpdatedDate = DateTime.UtcNow.ToJSONString(),
                Email = "testuser@mydomain.com"
            };

            Assert.IsFalse(await _dbUserManager.UserExistsAsync(TEST_ORG_ID2, TEST_USER_ID2), "user should not exist");
            AssertValid(await _dbUserManager.AddAppUserToOrgAsyncAsync(TEST_ORG_ID2, appUser));
            AssertValid(await _dbUserManager.AddAppUserToOrgAsyncAsync(TEST_ORG_ID3, appUser));
            Assert.IsTrue(await _dbUserManager.UserExistsAsync(TEST_ORG_ID2, TEST_USER_ID2), "user did not get created");
            Assert.IsTrue(await _dbUserManager.UserExistsAsync(TEST_ORG_ID3, TEST_USER_ID2), "user did not get created");

            // Confirm the record is in the dev shared sql server, not in postress
            Assert.AreEqual(1, (await _devShared.AppUser.Where(usr => usr.AppUserId == TEST_USER_ID2).ToListAsync()).Count(), "Should only have on user on SQL Server.");
        }

        [TestMethod]
        public async Task AddOrgSQLServer()
        {
            var org = new Organization()
            {
                Id = TEST_ORG_ID2,
                Name = "Test Org",
                BillingContact = UserEH2,
                Status = "Active",
                CreationDate = DateTime.UtcNow.ToJSONString(),
                LastUpdatedDate = DateTime.UtcNow.ToJSONString(),
            };

            var appUser = new AppUser()
            {
                Id = TEST_USER_ID2,
                FirstName = "Test",
                LastName = "User",
                CreationDate = DateTime.UtcNow.ToJSONString(),
                LastUpdatedDate = DateTime.UtcNow.ToJSONString(),
                Email = "testuser@mydomain.com"
            };

            AssertValid(await _dbUserManager.AddAppUserToOrgAsyncAsync(TEST_ORG_ID2, appUser));
            Assert.IsFalse(await _dbUserManager.OrgExistsAsync(TEST_ORG_ID2), "org should not exist");
            AssertValid(await _dbUserManager.AddOrgAsync(org));
            Assert.IsTrue(await _dbUserManager.OrgExistsAsync(TEST_ORG_ID2), "org did not get created.");
            // Confirm the record is in the dev shared sql server, not in postress
            Assert.AreEqual(1, (await _devShared.Org.Where(usr => usr.OrgId == TEST_ORG_ID2).ToListAsync()).Count(), "Did not created org on SQL Server.");
        }

        [TestMethod]
        public async Task AddOrgPostgres()
        {
            var org = new Organization()
            {
                Id = TEST_ORG_ID1,
                Name = "Test Org",
                BillingContact = UserEH1,
                Status = "Active",
                CreationDate = DateTime.UtcNow.ToJSONString(),
                LastUpdatedDate = DateTime.UtcNow.ToJSONString(),
            };

            var appUser = new AppUser()
            {
                Id = TEST_USER_ID1,
                FirstName = "Test",
                LastName = "User",
                CreationDate = DateTime.UtcNow.ToJSONString(),
                LastUpdatedDate = DateTime.UtcNow.ToJSONString(),
                Email = "testuser@mydomain.com"
            };

            AssertValid(await _dbUserManager.AddAppUserToOrgAsyncAsync(TEST_ORG_ID1, appUser));
            Assert.IsFalse(await _dbUserManager.OrgExistsAsync(TEST_ORG_ID1), "org should not exist");
            AssertValid(await _dbUserManager.AddOrgAsync(org));
            Assert.IsTrue(await _dbUserManager.OrgExistsAsync(TEST_ORG_ID1), "org did not get created.");

            // Confirm the record is in the dev shared sql server, not in postress
            Assert.AreEqual(1, (await _npDevDedicated.Org.Where(usr => usr.OrgId == TEST_ORG_ID1).ToListAsync()).Count(), "Did not created org on Postgress.");
        }

        [TestMethod]
        public async Task UpdateOrgPostgres()
        {
            var org = new Organization()
            {
                Id = TEST_ORG_ID1,
                Name = "Test Org",
                BillingContact = UserEH1,
                Status = "Active",
                CreationDate = DateTime.UtcNow.ToJSONString(),
                LastUpdatedDate = DateTime.UtcNow.ToJSONString(),
            };

            var appUser = new AppUser()
            {
                Id = TEST_USER_ID1,
                FirstName = "Test",
                LastName = "User",
                CreationDate = DateTime.UtcNow.ToJSONString(),
                LastUpdatedDate = DateTime.UtcNow.ToJSONString(),
                Email = "testuser@mydomain.com"
            };

            AssertValid(await _dbUserManager.AddAppUserToOrgAsyncAsync(TEST_ORG_ID1, appUser));
            AssertValid(await _dbUserManager.AddOrgAsync(org));

            org.Name = $"Updated {DateTime.Now}";
            AssertValid(await _dbUserManager.UpdateOrgAsync(org));

            var loadedOrg = await _npDevDedicated.Org.SingleOrDefaultAsync(org => org.OrgId == TEST_ORG_ID1);
            Assert.IsNotNull(loadedOrg, "Could not load org.");
            Assert.AreEqual(org.Name, loadedOrg.OrgName);
        }


        [TestMethod]
        public async Task UpdateOrgSQLServer()
        {
            var org = new Organization()
            {
                Id = TEST_ORG_ID2,
                Name = "Test Org",
                BillingContact = UserEH2,
                Status = "Active",
                CreationDate = DateTime.UtcNow.ToJSONString(),
                LastUpdatedDate = DateTime.UtcNow.ToJSONString(),
            };

            var appUser = new AppUser()
            {
                Id = TEST_USER_ID2,
                FirstName = "Test",
                LastName = "User",
                CreationDate = DateTime.UtcNow.ToJSONString(),
                LastUpdatedDate = DateTime.UtcNow.ToJSONString(),
                Email = "testuser@mydomain.com"
            };

            AssertValid(await _dbUserManager.AddAppUserToOrgAsyncAsync(TEST_ORG_ID2, appUser));
            AssertValid(await _dbUserManager.AddOrgAsync(org));

            org.Name = $"Updated {DateTime.Now}";
            AssertValid(await _dbUserManager.UpdateOrgAsync(org));

            var loadedOrg = await _devShared.Org.SingleOrDefaultAsync(org => org.OrgId == TEST_ORG_ID2);
            Assert.IsNotNull(loadedOrg, "Could not load org.");
            Assert.AreEqual(org.Name, loadedOrg.OrgName);
        }

        [TestMethod]
        public async Task HasBillingRecordsSQLServer()
        {
            // Just make sure the SQL runs.
            await _dbUserManager.HasBillingRecords(TEST_ORG_ID2);
        }


        [TestMethod]
        public async Task HasBillingRecordsPostgreSQL()
        {
            // Just make sure the SQL runs.
            await _dbUserManager.HasBillingRecords(TEST_ORG_ID1);
        }

        [TestMethod]
        public async Task UpdateAppUserPosgreSQLAsync()
        {
            var appUser = new AppUser()
            {
                Id = TEST_USER_ID1,
                FirstName = "Test",
                LastName = "User",
                CreationDate = DateTime.UtcNow.ToJSONString(),
                LastUpdatedDate = DateTime.UtcNow.ToJSONString(),
                Email = "testuser@mydomain.com"
            };

            AssertValid(await _dbUserManager.AddAppUserToOrgAsyncAsync(TEST_ORG_ID1, appUser));

            appUser.FirstName = $"Updated {DateTime.Now}";
            AssertValid(await _dbUserManager.UpdateAppUserAsync(TEST_ORG_ID1, appUser));

            var user = await _npDevDedicated.AppUser.SingleOrDefaultAsync(apu => apu.AppUserId == TEST_USER_ID1);
            Assert.IsNotNull(user, "Could not find user created on PostgreSQL");
            Assert.AreEqual(appUser.Name, user.FullName);
        }


        [TestMethod]
        public async Task UpdateAppuserSQLServerAsync()
        {
            var appUser = new AppUser()
            {
                Id = TEST_USER_ID2,
                FirstName = "Test",
                LastName = "User",
                CreationDate = DateTime.UtcNow.ToJSONString(),
                LastUpdatedDate = DateTime.UtcNow.ToJSONString(),
                Email = "testuser@mydomain.com"
            };

            AssertValid(await _dbUserManager.AddAppUserToOrgAsyncAsync(TEST_ORG_ID2, appUser));
            appUser.FirstName = $"Updated {DateTime.Now}";
            AssertValid(await _dbUserManager.UpdateAppUserAsync(TEST_ORG_ID2, appUser));

            var user = await _devShared.AppUser.SingleOrDefaultAsync(apu => apu.AppUserId == TEST_USER_ID2);
            Assert.IsNotNull(user, "Could not find user created in SQL Server");
            Assert.AreEqual(appUser.Name, user.FullName);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            await _npDevDedicated.Database.ExecuteSqlRawAsync($"delete from org where orgid = '{TEST_ORG_ID1}'");
            await _npDevDedicated.Database.ExecuteSqlRawAsync($"delete from org where orgid = '{TEST_ORG_ID2}'");
            await _npDevDedicated.Database.ExecuteSqlRawAsync($"delete from org where orgid = '{TEST_ORG_ID3}'");

            await _npDevDedicated.Database.ExecuteSqlRawAsync($"delete from appuser where appuserid = '{TEST_USER_ID1}'");
            await _npDevDedicated.Database.ExecuteSqlRawAsync($"delete from appuser where appuserid = '{TEST_USER_ID2}'");
            await _npDevDedicated.Database.ExecuteSqlRawAsync($"delete from appuser where appuserid = '{TEST_USER_ID3}'");

            await _devShared.Database.ExecuteSqlRawAsync($"delete from org where orgid = '{TEST_ORG_ID1}'");
            await _devShared.Database.ExecuteSqlRawAsync($"delete from org where orgid = '{TEST_ORG_ID2}'");
            await _devShared.Database.ExecuteSqlRawAsync($"delete from org where orgid = '{TEST_ORG_ID3}'");

            await _devShared.Database.ExecuteSqlRawAsync($"delete from appuser where appuserid = '{TEST_USER_ID1}'");
            await _devShared.Database.ExecuteSqlRawAsync($"delete from appuser where appuserid = '{TEST_USER_ID2}'");
            await _devShared.Database.ExecuteSqlRawAsync($"delete from appuser where appuserid = '{TEST_USER_ID3}'");
        }
    }
}
