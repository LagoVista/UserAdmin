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
            var one = AssociationRecordIdentity.Create("team-user", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            Assert.That(one.Value.Length, Is.EqualTo(32));
            Assert.That(one.Value,
                Is.EqualTo(AssociationRecordIdentity.Create("team-user", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB").Value));
            Assert.That(one.Value,
                Is.Not.EqualTo(AssociationRecordIdentity.Create("team-user", "team-2", "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB").Value));
            Assert.That(one.Value,
                Is.Not.EqualTo(AssociationRecordIdentity.Create("team-user", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "user-2").Value));
            Assert.That(one.Value,
                Is.Not.EqualTo(AssociationRecordIdentity.Create("managed-asset", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB").Value));
            Assert.That(AssociationRecordIdentity.Create("team-user", "a:b", "c").Value,
                Is.Not.EqualTo(AssociationRecordIdentity.Create("team-user", "a", "b:c").Value));
        }

        [Test]
        public void Team_Membership_Retains_User_And_Team_Summary()
        {
            var member = NewMember();
            var saved = TeamMembershipRecord.FromModel(member, PlatformOrg);
            Assert.That(saved.TeamId, Is.EqualTo("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"));
            Assert.That(saved.UserId, Is.EqualTo("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"));
            Assert.That(saved.Organization.Id, Is.EqualTo(PlatformOrg.Id));
            Assert.That(saved.Member.PartitionKey, Is.EqualTo("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"));
            Assert.That(saved.Member.RowKey, Is.EqualTo(TeamUser.CreateRowKey("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB")));
            Assert.That(saved.Member.CreateSummary().Name, Is.EqualTo("Ada"));
            Assert.That(saved.Member.CreateSummary().ProfileImageUrl, Is.EqualTo("/ada.png"));
        }

        [Test]
        public void Managed_Asset_Normalizes_Old_Missing_Partition_Without_Requiring_Table_Storage()
        {
            var member = NewAsset();
            Assert.That(member.PartitionKey, Is.Null.Or.Empty);
            var saved = ManagedAssetAssociationRecord.FromModel(member, PlatformOrg);
            Assert.That(saved.AssetSetId, Is.EqualTo("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"));
            Assert.That(saved.AssetId, Is.EqualTo("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD"));
            Assert.That(saved.Association.PartitionKey, Is.EqualTo("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"));
            Assert.That(saved.Association.RowKey,
                Is.EqualTo(ManagedAsset.CreateRowKey("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC", "DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD")));
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
            var teams = (await repo.GetMembersTeamsAsync("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB")).ToList();
            var members = (await repo.GetTeamMembersAsync("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")).ToList();

            Assert.That(teams.Single().Id.Value, Is.EqualTo("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"));
            Assert.That(members.Single().Name, Is.EqualTo("Ada"));
            Assert.That(queries.Count, Is.EqualTo(4));
            Assert.That(cursors, Is.EqualTo(new[] { null, "continue", null, "continue" }));
            Assert.That(queries[0].Filters.Any(f => f.Field == nameof(TeamMembershipRecord.UserId) &&
                (string)f.Value == "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), Is.True);
            Assert.That(queries[2].Filters.Any(f => f.Field == nameof(TeamMembershipRecord.TeamId) &&
                (string)f.Value == "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Is.True);
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
            var sets = (await repo.GetAssetSetsForManagedAssetAsync("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD")).ToList();
            var assets = (await repo.GetManagedAssetsAsync("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")).ToList();

            Assert.That(sets.Single().Id.Value, Is.EqualTo("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"));
            Assert.That(assets.Single().Id.Value, Is.EqualTo("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD"));
            Assert.That(assets.Single().AssetType, Is.EqualTo("camera"));
            Assert.That(queries[0].Filters.Any(f => f.Field == nameof(ManagedAssetAssociationRecord.AssetId) &&
                (string)f.Value == "DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD"), Is.True);
            Assert.That(queries[1].Filters.Any(f => f.Field == nameof(ManagedAssetAssociationRecord.AssetSetId) &&
                (string)f.Value == "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), Is.True);
        }

        [Test]
        public async Task Delete_Uses_Same_Deterministic_Key_As_Insert()
        {
            var teamStore = NewStore();
            var teamRepo = new TeamUserRepo(teamStore.Object, NewConfig().Object);
            await teamRepo.RemoveMemberAsync("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            teamStore.Verify(store => store.DeleteAsync<TeamMembershipRecord>(
                It.Is<StorageKey>(key =>
                    key.Id == AssociationRecordIdentity.Create("team-user", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB").Value &&
                    key.Scope == PlatformOrg.Id),
                It.IsAny<CancellationToken>()), Times.Once());

            var assetStore = NewStore();
            var assetRepo = new ManagedAssetRepo(assetStore.Object, NewConfig().Object);
            await assetRepo.RemoveManagedAssetAsync("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC", "DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            assetStore.Verify(store => store.DeleteAsync<ManagedAssetAssociationRecord>(
                It.Is<StorageKey>(key =>
                    key.Id == AssociationRecordIdentity.Create("managed-asset", "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC", "DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD").Value &&
                    key.Scope == PlatformOrg.Id),
                It.IsAny<CancellationToken>()), Times.Once());
        }

        private static TeamUser NewMember()
        {
            return new TeamUser(EntityHeader.Create("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "Team One"),
                EntityHeader.Create("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB", "Ada"))
            {
                ProfileImageUrl = "/ada.png"
            };
        }

        private static ManagedAsset NewAsset()
            => new ManagedAsset(EntityHeader.Create("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC", "Asset Set"),
                EntityHeader.Create("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD", "Camera"), "camera");

        private static Mock<IApplicationDataStore> NewStore() => new Mock<IApplicationDataStore>();

        private static Mock<IAppConfig> NewConfig()
        {
            var config = new Mock<IAppConfig>();
            config.SetupGet(x => x.SystemOwnerOrg).Returns(PlatformOrg);
            return config;
        }
    }
}
