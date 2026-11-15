using LagoVista.CloudStorage.Storage;
using LagoVista.Core;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Repos.ApplicationData.AnonymousVisitors;
using NUnit.Framework;
using System;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public sealed class AnonymousVisitorApplicationDataTests
    {
        private static readonly EntityHeader SystemOrg = new EntityHeader
        {
            Id = "82A7FA17E3E44920A2C12E08F327D589",
            Text = "Platform"
        };

        [Test]
        public void Record_Uses_Actor_Identity_And_One_Mongo_Collection()
        {
            var visitor = ActiveVisitor();
            var record = AnonymousVisitorRecord.Create(visitor, SystemOrg);

            Assert.That(StorageRecordIdentity.GetCollectionName<AnonymousVisitorRecord>(),
                Is.EqualTo("AnonymousVisitorRecord"));
            Assert.That(record.Id.Value, Is.EqualTo(visitor.ActorId));
            Assert.That(record.Organization.Id, Is.EqualTo(SystemOrg.Id));
            Assert.That(record.Visitor, Is.SameAs(visitor));
            Assert.That(record.StateKey, Is.EqualTo("Active"));
            Assert.That(record.LifecycleDueUtc, Is.EqualTo(visitor.ExpiresUtc));
            Assert.That(AnonymousVisitorRepo.Retention, Is.EqualTo(TimeSpan.FromDays(180)));
        }

        [Test]
        public void Record_Uses_Promotion_Time_As_Lifecycle_Due_Date()
        {
            var visitor = ActiveVisitor();
            visitor.State = AnonymousVisitorState.Promoted;
            visitor.PromotedUtc = DateTime.UtcNow.AddMinutes(-4);
            visitor.StateChangedUtc = DateTime.UtcNow.AddMinutes(-4);
            visitor.ProvisionalEnvironmentId = Guid.NewGuid().ToId();

            var record = AnonymousVisitorRecord.Create(visitor, SystemOrg);

            Assert.That(record.StateKey, Is.EqualTo("Promoted"));
            Assert.That(record.LifecycleDueUtc, Is.EqualTo(visitor.PromotedUtc.Value));
            Assert.That(record.Visitor.ProvisionalEnvironmentId, Is.EqualTo(visitor.ProvisionalEnvironmentId));
        }

        [Test]
        public void Record_Uses_Expiration_Time_As_Lifecycle_Due_Date()
        {
            var visitor = ActiveVisitor();
            visitor.State = AnonymousVisitorState.Expired;
            visitor.ExpiredUtc = DateTime.UtcNow.AddMinutes(-1);
            visitor.StateChangedUtc = visitor.ExpiredUtc.Value;

            var record = AnonymousVisitorRecord.Create(visitor, SystemOrg);

            Assert.That(record.StateKey, Is.EqualTo("Expired"));
            Assert.That(record.LifecycleDueUtc, Is.EqualTo(visitor.ExpiredUtc.Value));
        }

        [Test]
        public void Record_Rejects_Unnormalized_Actor_Id()
        {
            var visitor = ActiveVisitor();
            visitor.ActorId = "visitor-actor";

            Assert.Throws<ArgumentException>(() => AnonymousVisitorRecord.Create(visitor, SystemOrg));
        }

        private static AnonymousVisitor ActiveVisitor()
        {
            var now = DateTime.UtcNow;
            return new AnonymousVisitor
            {
                ActorId = Guid.NewGuid().ToId(),
                State = AnonymousVisitorState.Active,
                ContinuityTokenHash = "hash",
                CreatedUtc = now.AddMinutes(-1),
                LastActivityUtc = now,
                StateChangedUtc = now.AddMinutes(-1),
                ExpiresUtc = now.AddHours(24)
            };
        }
    }
}
