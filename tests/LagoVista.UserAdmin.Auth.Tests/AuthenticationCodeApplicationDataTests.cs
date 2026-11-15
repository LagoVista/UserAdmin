using LagoVista.CloudStorage.Storage;
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Repos.ApplicationData.AuthCodes;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public sealed class AuthenticationCodeApplicationDataTests
    {
        private static readonly EntityHeader SystemOrg = new EntityHeader
        {
            Id = "82A7FA17E3E44920A2C12E08F327D589",
            Text = "Platform"
        };

        [Test]
        public void SingleUseToken_Identity_Is_Deterministic_And_User_Scoped()
        {
            var a = SingleUseTokenRecord.CreateId("user-1", "token-1");
            Assert.That(a.Value, Is.EqualTo(SingleUseTokenRecord.CreateId("user-1", "token-1").Value));
            Assert.That(a.Value, Is.Not.EqualTo(SingleUseTokenRecord.CreateId("user-2", "token-1").Value));
            Assert.That(a.Value, Is.Not.EqualTo(SingleUseTokenRecord.CreateId("user-1", "token-2").Value));
            Assert.That(a.Value.Length, Is.EqualTo(32));
        }

        [Test]
        public void Both_Verification_Code_Kinds_Have_Bounded_Physical_Retention()
        {
            Assert.That(VerificationCodeRecord.Retention, Is.EqualTo(TimeSpan.FromDays(1)));
            Assert.That(SingleUseTokenRecord.Retention, Is.EqualTo(TimeSpan.FromDays(30)));
            Assert.That(VerificationCodeRecord.PasswordResetKind,
                Is.Not.EqualTo(VerificationCodeRecord.EmailVerificationKind));
        }

        [Test]
        public async Task Store_PasswordResetCode_Persists_Hash_And_Attempt_State()
        {
            var store = NewStore();
            VerificationCodeRecord captured = null;
            store.Setup(s => s.InsertAsync(It.IsAny<VerificationCodeRecord>(),
                It.IsAny<CancellationToken>()))
                .Callback<VerificationCodeRecord, CancellationToken>((record, _) => captured = record)
                .Returns(Task.CompletedTask);

            var now = DateTime.UtcNow;
            var code = new PasswordResetCode
            {
                Id = Guid.NewGuid().ToId(),
                UserId = Guid.NewGuid().ToId(),
                CodeHash = "test-password-hash",
                CreatedUtc = now,
                ExpiresUtc = now.AddMinutes(10),
                AttemptCount = 2
            };
            await new PasswordResetCodeRepo(store.Object, NewConfig().Object).StoreAsync(code);

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured.Id.Value, Is.EqualTo(code.Id));
            Assert.That(captured.Organization.Id, Is.EqualTo(SystemOrg.Id));
            Assert.That(captured.Kind, Is.EqualTo(VerificationCodeRecord.PasswordResetKind));
            Assert.That(captured.CodeHash, Is.EqualTo(code.CodeHash));
            Assert.That(captured.AttemptCount, Is.EqualTo(2));
            Assert.That(captured.ExpiresUtc, Is.EqualTo(code.ExpiresUtc));
        }

        [Test]
        public async Task Store_EmailVerificationCode_Keeps_Separate_Kind()
        {
            var store = NewStore();
            VerificationCodeRecord captured = null;
            store.Setup(s => s.InsertAsync(It.IsAny<VerificationCodeRecord>(),
                It.IsAny<CancellationToken>()))
                .Callback<VerificationCodeRecord, CancellationToken>((record, _) => captured = record)
                .Returns(Task.CompletedTask);

            var now = DateTime.UtcNow;
            var code = new EmailVerificationCode
            {
                Id = Guid.NewGuid().ToId(),
                UserId = Guid.NewGuid().ToId(),
                CodeHash = "test-email-hash",
                CreatedUtc = now,
                ExpiresUtc = now.AddMinutes(10)
            };
            await new EmailVerificationCodeRepo(store.Object, NewConfig().Object).StoreAsync(code);

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured.Kind, Is.EqualTo(VerificationCodeRecord.EmailVerificationKind));
            Assert.That(captured.UserId, Is.EqualTo(code.UserId));
            Assert.That(captured.CodeHash, Is.EqualTo(code.CodeHash));
        }

        [Test]
        public async Task Store_SingleUseToken_Persists_Hashed_Lookup_Identity()
        {
            var store = NewStore();
            SingleUseTokenRecord captured = null;
            store.Setup(s => s.InsertAsync(It.IsAny<SingleUseTokenRecord>(),
                It.IsAny<CancellationToken>()))
                .Callback<SingleUseTokenRecord, CancellationToken>((record, _) => captured = record)
                .Returns(Task.CompletedTask);

            var token = new SingleUseToken
            {
                UserId = Guid.NewGuid().ToId(),
                Token = "test-one-time-token",
                Expires = DateTime.UtcNow.AddMinutes(1).ToJSONString()
            };
            await new SingleUseTokenRepo(store.Object, NewConfig().Object).StoreAsync(token);

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured.Id.Value, Is.EqualTo(
                SingleUseTokenRecord.CreateId(token.UserId, token.Token).Value));
            Assert.That(captured.UserId, Is.EqualTo(token.UserId));
            Assert.That(captured.Token, Is.EqualTo(token.Token));
            Assert.That(captured.ConsumedUtc, Is.Null);
        }

        private static Mock<IAppConfig> NewConfig()
        {
            var config = new Mock<IAppConfig>();
            config.SetupGet(c => c.SystemOwnerOrg).Returns(SystemOrg);
            return config;
        }

        private static Mock<IApplicationDataStore> NewStore() => new Mock<IApplicationDataStore>();
    }
}
