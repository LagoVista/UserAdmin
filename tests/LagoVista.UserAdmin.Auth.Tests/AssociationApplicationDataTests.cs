using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Repos.ApplicationData.Associations;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public sealed class AssociationApplicationDataTests
    {
        private static readonly EntityHeader PlatformOrg = new EntityHeader
        {
            Id = "82A7FA17E3E44920A2C12E08F327D589",
            Text = "Platform"
        };

        [Test]
        public void Association_Ids_Are_Deterministic_And_Separate_Both_Scopes()
        {
            var one = AssociationRecordIdentity.Create("team-user", "team-1", "user-1");
            Assert.That(one.Value.Length, Is.EqualTo(32));
            Assert.That(one.Value,
                Is.EqualTo(AssociationRecordIdentity.Create("team-user", "team-1", "user-1").Value));
            Assert.That(one.Value,
                Is.Not.EqualTo(AssociationRecordIdentity.Create("team-user", "team-2", "user-1").Value));
            Assert.That(one.Value,
                Is.Not.EqualTo(AssociationRecordIdentity.Create("team-user", "team-1", "user-2").Value));
            Assert.That(one.Value,
                Is.Not.EqualTo(AssociationRecordIdentity.Create("managed-asset", "team-1", "user-1").Value));
            Assert.That(AssociationRecordIdentity.Create("team-user", "a:b", "c").Value,
                Is.Not.EqualTo(AssociationRecordIdentity.Create("team-user", "a", "b:c").Value));
        }

        [Test]
        public void Team_Membership_Retains_User_And_Team_Summary()
        {
            var member = NewMember();
            var saved = TeamMembershipRecord.FromModel(member, PlatformOrg);
            Assert.That(saved.TeamId, Is.EqualTo("team-1"));
            Assert.That(saved.UserId, Is.EqualTo("user-1"));
            Assert.That(saved.Organization.Id, Is.EqualTo(PlatformOrg.Id));
            Assert.That(saved.Member.PartitionKey, Is.EqualTo("team-1"));
            Assert.That(saved.Member.RowKey, Is.EqualTo(TeamUser.CreateRowKey("team-1", "user-1")));
            Assert.That(saved.Member.CreateSummary().Name, Is.EqualTo("Ada"));
            Assert.That(saved.Member.CreateSummary().ProfileImageUrl, Is.EqualTo("/ada.png"));
        }

        [Test]
        public void Managed_Asset_Normalizes_Old_Missing_Partition_Without_Requiring_Table_Storage()
        {
            var member = NewAsset();
            Assert.That(member.PartitionKey, Is.Null.Or.Empty);
            var saved = ManagedAssetAssociationRecord.FromModel(member, PlatformOrg);
            Assert.That(saved.AssetSetId, Is.EqualTo("set-1"));
            Assert.That(saved.AssetId, Is.EqualTo("asset-1"));
            Assert.That(saved.Association.PartitionKey, Is.EqualTo("set-1"));
            Assert.That(saved.Association.RowKey,
                Is.EqualTo(ManagedAsset.CreateRowKey("set-1", "asset-1")));
            Assert.That(saved.Association.CreateSummary().AssetType, Is.EqualTo("camera"));
        }

        [Test]
        public void Mismatched_Association_Scope_Is_Rejected()
        {
            var member = NewMember();
            member.PartitionKey = "not-team-1";
            Assert.Throws<ArgumentException>(() => TeamMembershipRecord.FromModel(member, PlatformOrg));
            var asset = NewAsset();
            asset.RowKey = "not-the-pair";
            Assert.Throws<ArgumentException>(() => ManagedAssetAssociationRecord.FromModel(asset, PlatformOrg));
        }

        [Test]
        public async Task Team_Queries_Use_Both_Directions_And_Continue_Until_Complete()
        {
            var member = TeamMembershipRecord.FromModel(NewMember(), PlatformOrg);
            var store = NewStore();
            var queries = new List<StorageQuery<TeamMembershipRecord>>();
            var cursors = new List<string>();
            store.Setup(s => s.QueryAsync(It.IsAny<StorageQuery<TeamMembershipRecord>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((StorageQuery<TeamMembershipRecord> query, CancellationToken _) =>
                {
                    queries.Add(query);
                    cursors.Add(query.Page.ContinuationToken);
                    // Both directions should exhaust provider-owned cursors.
                    return new StoragePageResult<TeamMembershipRecord>(
                        query.Page.ContinuationToken == null ? new[] { member } : new TeamMembershipRecord[0],
                        query.Page.ContinuationToken == null ? "continue" : null);
                });
            var repo = new TeamUserRepo(store.Object, NewConfig().Object);
            var teams = (await repo.GetMembersTeamsAsync("user-1")).ToList();
            var members = (await repo.GetTeamMembersAsync("team-1")).ToList();

            Assert.That(teams.Single().Id, Is.EqualTo("team-1"));
            Assert.That(members.Single().Name, Is.EqualTo("Ada"));
            Assert.That(queries.Count, Is.EqualTo(4));
            Assert.That(cursors, Is.EqualTo(new[] { null, "continue", null, "continue" }));
            Assert.That(queries[0].Filters.Any(f => f.Field == nameof(TeamMembershipRecord.UserId) &&
                (string)f.Value == "user-1"), Is.True);
            Assert.That(queries[2].Filters.Any(f => f.Field == nameof(TeamMembershipRecord.TeamId) &&
                (string)f.Value == "team-1"), Is.True);
            Assert.That(queries.All(q => q.Filters.Any(f => f.Field == "Organization.Id" &&
                (string)f.Value == PlatformOrg.Id)), Is.True);
        }

        [Test]
        public async Task Managed_Asset_Queries_Use_Both_Directions()
        {
            var asset = ManagedAssetAssociationRecord.FromModel(NewAsset(), PlatformOrg);
            var store = NewStore();
            var queries = new List<StorageQuery<ManagedAssetAssociationRecord>>();
            store.Setup(s => s.QueryAsync(It.IsAny<StorageQuery<ManagedAssetAssociationRecord>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((StorageQuery<ManagedAssetAssociationRecord> query, CancellationToken _) =>
                {
                    queries.Add(query);
                    return new StoragePageResult<ManagedAssetAssociationRecord>(new[] { asset });
                });
            var repo = new ManagedAssetRepo(store.Object, NewConfig().Object);
            var sets = (await repo.GetAssetSetsForManagedAssetAsync("asset-1")).ToList();
            var assets = (await repo.GetManagedAssetsAsync("set-1")).ToList();

            Assert.That(sets.Single().Id, Is.EqualTo("set-1"));
            Assert.That(assets.Single().Id, Is.EqualTo("asset-1"));
            Assert.That(assets.Single().AssetType, Is.EqualTo("camera"));
            Assert.That(queries[0].Filters.Any(f => f.Field == nameof(ManagedAssetAssociationRecord.AssetId) &&
                (string)f.Value == "asset-1"), Is.True);
            Assert.That(queries[1].Filters.Any(f => f.Field == nameof(ManagedAssetAssociationRecord.AssetSetId) &&
                (string)f.Value == "set-1"), Is.True);
        }

        [Test]
        public async Task Delete_Uses_Same_Deterministic_Key_As_Insert()
        {
            var teamStore = NewStore();
            var teamRepo = new TeamUserRepo(teamStore.Object, NewConfig().Object);
            await teamRepo.RemoveMemberAsync("team-1", "user-1");
            teamStore.Verify(store => store.DeleteAsync<TeamMembershipRecord>(
                It.Is<StorageKey>(key =>
                    key.Id == AssociationRecordIdentity.Create("team-user", "team-1", "user-1").Value &&
                    key.Scope == PlatformOrg.Id),
                It.IsAny<CancellationToken>()), Times.Once());

            var assetStore = NewStore();
            var assetRepo = new ManagedAssetRepo(assetStore.Object, NewConfig().Object);
            await assetRepo.RemoveManagedAssetAsync("set-1", "asset-1");
            assetStore.Verify(store => store.DeleteAsync<ManagedAssetAssociationRecord>(
                It.Is<StorageKey>(key =>
                    key.Id == AssociationRecordIdentity.Create("managed-asset", "set-1", "asset-1").Value &&
                    key.Scope == PlatformOrg.Id),
                It.IsAny<CancellationToken>()), Times.Once());
        }

        private static TeamUser NewMember()
        {
            return new TeamUser(EntityHeader.Create("team-1", "Team One"),
                EntityHeader.Create("user-1", "Ada"))
            {
                ProfileImageUrl = "/ada.png"
            };
        }

        private static ManagedAsset NewAsset()
            => new ManagedAsset(EntityHeader.Create("set-1", "Asset Set"),
                EntityHeader.Create("asset-1", "Camera"), "camera");

        private static Mock<IApplicationDataStore> NewStore() => new Mock<IApplicationDataStore>();

        private static Mock<IAppConfig> NewConfig()
        {
            var config = new Mock<IAppConfig>();
            config.SetupGet(x => x.SystemOwnerOrg).Returns(PlatformOrg);
            return config;
        }
    }
}
