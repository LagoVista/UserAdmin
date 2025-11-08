// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d70c26d6e27f1a6174f8eb2133c8fe138c83a41accf62cfc1fdbf8cd58915d4d
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Exceptions;
using LagoVista.Core.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.Logging.Utils;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Repos.RDBMS;
using Microsoft.EntityFrameworkCore;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Tests.SubscriptionTests
{
    [TestClass]
    public class Subscriptions : TestBase
    {
        private ISubscriptionRepo _subscriptionManager;
        Mock<IRDBMSConnectionProvider> _cp = new Mock<IRDBMSConnectionProvider>();
        IAdminLogger _adminLogger = new AdminLogger(new ConsoleLogWriter());

        UserAdminDataContext _devShared;
        UserAdminDataContext _npDevDedicated;

        [TestInitialize]
        public async Task Init()
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

            _subscriptionManager = new SubscriptionRepo(_devShared, _cp.Object);

            _cp.Setup(cp => cp.GetConnectionAsync(It.IsAny<string>())).ReturnsAsync(new RDBMSConnection(RDBMSConnectionType.PosgreSQLDedicated, connstr) { VerboseLogging = true });
            _cp.Setup(cp => cp.GetConnectionAsync(It.Is<string>(s => s == TEST_ORG_ID2))).ReturnsAsync(new RDBMSConnection(RDBMSConnectionType.SQLServerShared));
            _cp.Setup(cp => cp.GetConnectionAsync(It.Is<string>(s => s == TEST_ORG_ID3))).ReturnsAsync(new RDBMSConnection(RDBMSConnectionType.SQLServerShared));
            var dbUserManager = new RDBMSManager(_devShared, _cp.Object, _adminLogger);


            await Cleanup();

            var orgPG = new Organization()
            {
                Id = TEST_ORG_ID1,
                Name = "Test Org",
                BillingContact = UserEH1,
                Status = "Active",
                CreationDate = DateTime.UtcNow.ToJSONString(),
                LastUpdatedDate = DateTime.UtcNow.ToJSONString(),
            };

            var orgSS = new Organization()
            {
                Id = TEST_ORG_ID2,
                Name = "Test Org",
                BillingContact = UserEH2,
                Status = "Active",
                CreationDate = DateTime.UtcNow.ToJSONString(),
                LastUpdatedDate = DateTime.UtcNow.ToJSONString(),
            };


            var appUserPG = new AppUser()
            {
                Id = TEST_USER_ID1,
                FirstName = "Test",
                LastName = "User",
                CreationDate = DateTime.UtcNow.ToJSONString(),
                LastUpdatedDate = DateTime.UtcNow.ToJSONString(),
                Email = "testuser@mydomain.com"
            };

            var appUserSS = new AppUser()
            {
                Id = TEST_USER_ID2,
                FirstName = "Test",
                LastName = "User",
                CreationDate = DateTime.UtcNow.ToJSONString(),
                LastUpdatedDate = DateTime.UtcNow.ToJSONString(),
                Email = "testuser@mydomain.com"
            };

            await dbUserManager.AddAppUserToOrgAsyncAsync(TEST_ORG_ID1, appUserPG);
            await dbUserManager.AddAppUserToOrgAsyncAsync(TEST_ORG_ID2, appUserSS);

            await dbUserManager.AddOrgAsync(orgPG);
            await dbUserManager.AddOrgAsync(orgSS);
        }

        private SubscriptionDTO Create(string orgId, string userId, string key)
        {
            var subscription = new SubscriptionDTO()
            {
                OrgId = orgId,
                CreatedById = userId,
                LastUpdatedById = userId,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                Name = $"Created {DateTime.UtcNow}",
                Key = key,
                PaymentToken = "ABC123",
                PaymentTokenStatus = "ACTIVE",
                Icon = "NA",
                Status = "ok"
            };

            return subscription;
        }


        [TestMethod]
        public async Task AddAndGetSubscriptionSS()
        {
            var subscription = Create(TEST_ORG_ID2, TEST_USER_ID2, "key");
            await _subscriptionManager.AddSubscriptionAsync(subscription);

            var loadedSubscription = await _subscriptionManager.GetSubscriptionAsync(TEST_ORG_ID2, subscription.Id);
            Assert.IsNotNull(loadedSubscription);
            Assert.AreEqual(subscription.Name, loadedSubscription.Name);
        }

        [TestMethod]
        public async Task AddAndGetSubscriptionPG()
        {
            var subscription = Create(TEST_ORG_ID1, TEST_USER_ID1, "key");
            await _subscriptionManager.AddSubscriptionAsync(subscription);
    
            var loadedSubscription = await _subscriptionManager.GetSubscriptionAsync(TEST_ORG_ID1, subscription.Id);
            Assert.IsNotNull(loadedSubscription);
            Assert.AreEqual(subscription.Name, loadedSubscription.Name);
        }

        [TestMethod]
        public async Task UpdateSubscriptionSS()
        {
            var subscription = Create(TEST_ORG_ID2, TEST_USER_ID2, "key");
            await _subscriptionManager.AddSubscriptionAsync(subscription);
            var loadedSubscription = await _devShared.Subscription.SingleAsync(subs => subs.OrgId == TEST_ORG_ID2);

            loadedSubscription.Name = $"Updated {DateTime.Now}";
            await _subscriptionManager.UpdateSubscriptionAsync(loadedSubscription);

            var reLoadedSubscriptions = await _devShared.Subscription.Where(subs => subs.OrgId == TEST_ORG_ID2).ToListAsync();
            Assert.AreEqual(1, reLoadedSubscriptions.Count);
            Assert.IsNotNull(reLoadedSubscriptions.Single());
            Assert.AreEqual(loadedSubscription.Name, reLoadedSubscriptions.Single().Name);
        }


        [TestMethod]
        public async Task UpdateSubscriptionPG()
        {
            var subscription = Create(TEST_ORG_ID1, TEST_USER_ID1, "key");
            await _subscriptionManager.AddSubscriptionAsync(subscription);
            var loadedSubscription = await _npDevDedicated.Subscription.SingleAsync(subs => subs.OrgId == TEST_ORG_ID1);

            loadedSubscription.Name = $"Updated {DateTime.Now}";
            await _subscriptionManager.UpdateSubscriptionAsync(loadedSubscription);

            var reLoadedSubscriptions = await _npDevDedicated.Subscription.Where(subs => subs.OrgId == TEST_ORG_ID1).ToListAsync();
            Assert.AreEqual(1, reLoadedSubscriptions.Count);
            Assert.IsNotNull(reLoadedSubscriptions.Single());
            Assert.AreEqual(loadedSubscription.Name, reLoadedSubscriptions.Single().Name);
        }

        [TestMethod]
        public async Task AddAndGetTrialSubscriptionPG()
        {
            var subscription = Create(TEST_ORG_ID1, TEST_USER_ID1, SubscriptionDTO.SubscriptionKey_Trial);
            await _subscriptionManager.AddSubscriptionAsync(subscription);

            var loadedSubscription = await _subscriptionManager.GetTrialSubscriptionAsync(TEST_ORG_ID1);
            Assert.IsNotNull(loadedSubscription);
            Assert.AreEqual(SubscriptionDTO.SubscriptionKey_Trial, subscription.Key);

            var reLoadedSubscriptions = await _npDevDedicated.Subscription.Where(subs => subs.Id == subscription.Id).SingleOrDefaultAsync();
            Assert.IsNotNull(loadedSubscription, "Did not find in SQL Server.");
        }

        [TestMethod]
        public async Task AddAndGetTrialSubscriptionSS()
        {
            var subscription = Create(TEST_ORG_ID2, TEST_USER_ID2, SubscriptionDTO.SubscriptionKey_Trial);
            await _subscriptionManager.AddSubscriptionAsync(subscription);

            var loadedSubscription = await _subscriptionManager.GetTrialSubscriptionAsync(TEST_ORG_ID2);
            Assert.IsNotNull(loadedSubscription);
            Assert.AreEqual(SubscriptionDTO.SubscriptionKey_Trial, subscription.Key);

            var reLoadedSubscriptions = await _devShared.Subscription.Where(subs => subs.Id == subscription.Id).SingleOrDefaultAsync();
            Assert.IsNotNull(loadedSubscription, "Did not find in SQL Server.");
        }

        [TestMethod]
        public async Task AddAndGetMultipleSubscriptionsSS()
        {
            var subscription1 = Create(TEST_ORG_ID2, TEST_USER_ID2, SubscriptionDTO.SubscriptionKey_Trial);
            await _subscriptionManager.AddSubscriptionAsync(subscription1);
            var subscription2 = Create(TEST_ORG_ID2, TEST_USER_ID2, "key1");
            await _subscriptionManager.AddSubscriptionAsync(subscription2);
            var subscription3 = Create(TEST_ORG_ID2, TEST_USER_ID2, "key2");
            await _subscriptionManager.AddSubscriptionAsync(subscription3);

            var subscriptions = await _subscriptionManager.GetSubscriptionsForOrgAsync(TEST_ORG_ID2);
            Assert.AreEqual(3, subscriptions.Count());

            var reLoadedSubscriptions = await _devShared.Subscription.Where(subs => subs.OrgId == TEST_ORG_ID2).ToListAsync();
            Assert.AreEqual(3, reLoadedSubscriptions.Count());
        }

        [TestMethod]
        public async Task DeletesSubscriptionsPG()
        {
            var subscription1 = Create(TEST_ORG_ID1, TEST_USER_ID1, SubscriptionDTO.SubscriptionKey_Trial);
            await _subscriptionManager.AddSubscriptionAsync(subscription1);
            var subscription2 = Create(TEST_ORG_ID1, TEST_USER_ID1, "key1");
            await _subscriptionManager.AddSubscriptionAsync(subscription2);
            var subscription3 = Create(TEST_ORG_ID1, TEST_USER_ID1, "key2");
            await _subscriptionManager.AddSubscriptionAsync(subscription3);

            var reLoadedSubscriptions = await _npDevDedicated.Subscription.Where(subs => subs.OrgId == TEST_ORG_ID1).ToListAsync();
            Assert.AreEqual(3, reLoadedSubscriptions.Count());

            await _subscriptionManager.DeleteSubscriptionsForOrgAsync(TEST_ORG_ID1);

            reLoadedSubscriptions = await _npDevDedicated.Subscription.Where(subs => subs.OrgId == TEST_ORG_ID1).ToListAsync();
            Assert.AreEqual(0, reLoadedSubscriptions.Count());
        }

        [TestMethod]
        public async Task DeleteSubscriptionsForOrgSS()
        {
            var subscription1 = Create(TEST_ORG_ID2, TEST_USER_ID2, SubscriptionDTO.SubscriptionKey_Trial);
            await _subscriptionManager.AddSubscriptionAsync(subscription1);
            var subscription2 = Create(TEST_ORG_ID2, TEST_USER_ID2, "key1");
            await _subscriptionManager.AddSubscriptionAsync(subscription2);
            var subscription3 = Create(TEST_ORG_ID2, TEST_USER_ID2, "key2");
            await _subscriptionManager.AddSubscriptionAsync(subscription3);

            var reLoadedSubscriptions = await _devShared.Subscription.Where(subs => subs.OrgId == TEST_ORG_ID2).ToListAsync();
            Assert.AreEqual(3, reLoadedSubscriptions.Count());

            await _subscriptionManager.DeleteSubscriptionsForOrgAsync(TEST_ORG_ID2);

            reLoadedSubscriptions = await _devShared.Subscription.Where(subs => subs.OrgId == TEST_ORG_ID2).ToListAsync();
            Assert.AreEqual(0, reLoadedSubscriptions.Count());
        }

        [TestMethod]
        public async Task AddAndGetMultipleSubscriptionsPG()
        {
            var subscription1 = Create(TEST_ORG_ID1, TEST_USER_ID1, SubscriptionDTO.SubscriptionKey_Trial);
            await _subscriptionManager.AddSubscriptionAsync(subscription1);
            var subscription2 = Create(TEST_ORG_ID1, TEST_USER_ID1, "key1");
            await _subscriptionManager.AddSubscriptionAsync(subscription2);
            var subscription3 = Create(TEST_ORG_ID1, TEST_USER_ID1, "key2");
            await _subscriptionManager.AddSubscriptionAsync(subscription3);

            var subscriptions = await _subscriptionManager.GetSubscriptionsForOrgAsync(TEST_ORG_ID1);
            Assert.AreEqual(3, subscriptions.Count());

            var reLoadedSubscriptions = await _npDevDedicated.Subscription.Where(subs => subs.OrgId == TEST_ORG_ID1).ToListAsync();
            Assert.AreEqual(3, reLoadedSubscriptions.Count());
        }

        [TestMethod]
        public async Task DeleteSubscriptionPG()
        {
            var subscription = Create(TEST_ORG_ID1, TEST_USER_ID1, "key1");
            await _subscriptionManager.AddSubscriptionAsync(subscription);
            var reLoadedSubscriptions = await _npDevDedicated.Subscription.Where(subs => subs.OrgId == TEST_ORG_ID1).ToListAsync();
            Assert.AreEqual(1, reLoadedSubscriptions.Count());
            await _subscriptionManager.DeleteSubscriptionAsync(TEST_ORG_ID1, subscription.Id);
            reLoadedSubscriptions = await _npDevDedicated.Subscription.Where(subs => subs.OrgId == TEST_ORG_ID1).ToListAsync();
            Assert.AreEqual(0, reLoadedSubscriptions.Count());
        }

        [TestMethod]
        public async Task DeleteSubscriptionSS()
        {
            var subscription = Create(TEST_ORG_ID2, TEST_USER_ID2, "key1");
            await _subscriptionManager.AddSubscriptionAsync(subscription);
            var reLoadedSubscriptions = await _devShared.Subscription.Where(subs => subs.OrgId == TEST_ORG_ID2).ToListAsync();
            Assert.AreEqual(1, reLoadedSubscriptions.Count());
            await _subscriptionManager.DeleteSubscriptionAsync(TEST_ORG_ID2, subscription.Id);
            reLoadedSubscriptions = await _devShared.Subscription.Where(subs => subs.OrgId == TEST_ORG_ID2).ToListAsync();
            Assert.AreEqual(0, reLoadedSubscriptions.Count());
        }



        [TestCleanup]
        public async Task Cleanup()
        {
            await _npDevDedicated.Database.ExecuteSqlRawAsync($"delete from subscription where orgid = '{TEST_ORG_ID1}'");
            await _devShared.Database.ExecuteSqlRawAsync($"delete from subscription where orgid = '{TEST_ORG_ID2}'");

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
