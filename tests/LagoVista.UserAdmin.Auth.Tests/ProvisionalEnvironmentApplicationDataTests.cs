using LagoVista.Core;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Repos.ApplicationData.ProvisionalEnvironments;
using NUnit.Framework;
using System;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public sealed class ProvisionalEnvironmentApplicationDataTests
    {
        private static readonly EntityHeader SystemOrganization = new EntityHeader
        {
            Id = "82A7FA17E3E44920A2C12E08F327D589",
            Text = "Platform"
        };

        [Test]
        public void CreationRequest_Maps_To_Stable_And_Unique_Record_Identity()
        {
            var first = ProvisionalEnvironmentRecord.IdFromCreationRequest("request-one");
            var again = ProvisionalEnvironmentRecord.IdFromCreationRequest("request-one");
            var second = ProvisionalEnvironmentRecord.IdFromCreationRequest("request-two");

            Assert.That(first, Is.EqualTo(again));
            Assert.That(first, Is.Not.EqualTo(second));
            Assert.That(first.Length, Is.EqualTo(32));
            Assert.That(NormalizedId32.TryCreate(first, out _), Is.True);
        }

        [Test]
        public void Record_Preserves_Provisional_State_And_Credential_Fields()
        {
            var environment = CreateEnvironment();
            var record = ProvisionalEnvironmentRecord.FromEnvironment(environment, SystemOrganization);

            Assert.That(record.Id.Value, Is.EqualTo(environment.Id));
            Assert.That(record.Organization.Id, Is.EqualTo(SystemOrganization.Id));
            Assert.That(record.Environment, Is.SameAs(environment));
            Assert.That(record.Environment.CreationRequestId, Is.EqualTo("request-one"));
            Assert.That(record.Environment.RecoveryTokenHash, Is.EqualTo("recovery-hash"));
            Assert.That(record.StateKey, Is.EqualTo("Active"));
            Assert.That(record.LifecycleDueUtc, Is.EqualTo(environment.ExpiresUtc));
        }

        [Test]
        public void Expired_State_Is_Due_On_PurgeAfterUtc_Not_ExpiresUtc()
        {
            var environment = CreateEnvironment();
            environment.State = ProvisionalEnvironmentState.Expired;
            environment.PurgeAfterUtc = DateTime.UtcNow.AddDays(30);

            var record = ProvisionalEnvironmentRecord.FromEnvironment(environment, SystemOrganization);

            Assert.That(record.StateKey, Is.EqualTo("Expired"));
            Assert.That(record.LifecycleDueUtc, Is.EqualTo(environment.PurgeAfterUtc.Value));
        }

        [Test]
        public void PurgePending_Without_Purge_Date_Remains_Queryable()
        {
            var environment = CreateEnvironment();
            environment.State = ProvisionalEnvironmentState.PurgePending;
            environment.PurgeAfterUtc = null;

            var record = ProvisionalEnvironmentRecord.FromEnvironment(environment, SystemOrganization);

            Assert.That(record.StateKey, Is.EqualTo("PurgePending"));
            Assert.That(record.LifecycleDueUtc, Is.EqualTo(DateTime.MaxValue));
        }

        private static ProvisionalEnvironment CreateEnvironment()
        {
            var now = DateTime.UtcNow;
            return new ProvisionalEnvironment
            {
                Id = ProvisionalEnvironmentRecord.IdFromCreationRequest("request-one"),
                CreationRequestId = "request-one",
                State = ProvisionalEnvironmentState.Active,
                RecoveryTokenHash = "recovery-hash",
                CreatedUtc = now,
                LastActivityUtc = now,
                StateChangedUtc = now,
                ExpiresUtc = now.AddDays(30)
            };
        }
    }
}
