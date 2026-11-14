using LagoVista.CloudStorage.Storage;
using LagoVista.Core;
using LagoVista.Core.Authentication.Models;
using LagoVista.UserAdmin.Repos.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Tests.TokenTests
{
    [TestClass]
    public class RefreshTokenRepoTests
    {
        private const string UserId = "CC648B3B51164A8296EB7092F312D5CB";
        private const string TokenId = "2516076175907706319.B49DE5B2F5034912B0A4E719E94605EF";

        [TestMethod]
        public async Task LegacyTokenIsImportedWithoutIssuingAnotherToken()
        {
            var legacy = new Mock<ILegacyRefreshTokenReader>();
            var store = new Mock<IConditionalOperationalDataStore<RefreshTokenOperationalRecord>>();
            var oldToken = NewToken();
            legacy.Setup(x => x.GetAsync(UserId, TokenId)).ReturnsAsync(oldToken);
            store.Setup(x => x.TryCreateAsync(It.IsAny<RefreshTokenOperationalRecord>(), It.IsAny<CancellationToken>()))
                .Returns((RefreshTokenOperationalRecord record, CancellationToken _) =>
                    Task.FromResult(new ConditionalMutationResult<RefreshTokenOperationalRecord>(true, record)));

            var repo = new RefreshTokenRepo(store.Object, legacy.Object);
            var returned = await repo.GetRefreshTokenAsync(TokenId, UserId);

            Assert.IsNotNull(returned);
            Assert.AreEqual(oldToken.AppId, returned.AppId);
            Assert.AreEqual(oldToken.AppInstanceId, returned.AppInstanceId);
            Assert.AreEqual(oldToken.RowKey, returned.RowKey);
            store.Verify(x => x.TryCreateAsync(
                It.Is<RefreshTokenOperationalRecord>(t => t.Id == TokenId && t.OrganizationId == UserId),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task ConsumedTombstoneCannotFallBackToLegacyTable()
        {
            var legacy = new Mock<ILegacyRefreshTokenReader>();
            var store = new Mock<IConditionalOperationalDataStore<RefreshTokenOperationalRecord>>();
            store.Setup(x => x.GetAsync(UserId, TokenId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RefreshTokenOperationalRecord { Id = TokenId, OrganizationId = UserId, Consumed = true, Version = 2 });

            var repo = new RefreshTokenRepo(store.Object, legacy.Object);
            Assert.IsNull(await repo.GetRefreshTokenAsync(TokenId, UserId));
            Assert.IsFalse(await repo.TryConsumeRefreshTokenAsync(TokenId, UserId));
            legacy.Verify(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task TwoConcurrentClaimsCanOnlyHaveOneWinner()
        {
            var legacy = new Mock<ILegacyRefreshTokenReader>();
            var store = new Mock<IConditionalOperationalDataStore<RefreshTokenOperationalRecord>>();
            var record = RefreshTokenOperationalRecord.FromToken(NewToken());
            record.Version = 1;
            record.CreationDate = DateTime.UtcNow;
            store.Setup(x => x.GetAsync(UserId, TokenId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(record);

            var calls = 0;
            store.Setup(x => x.TryReplaceAsync(
                    It.IsAny<RefreshTokenOperationalRecord>(), 1, It.IsAny<CancellationToken>()))
                .Returns((RefreshTokenOperationalRecord candidate, long _, CancellationToken ct) =>
                {
                    // Simulates the Cassandra IF version = 1 compare-and-set.
                    var won = Interlocked.Increment(ref calls) == 1;
                    return Task.FromResult(new ConditionalMutationResult<RefreshTokenOperationalRecord>(
                        won, won ? candidate : new RefreshTokenOperationalRecord
                        {
                            Id = TokenId, OrganizationId = UserId, Consumed = true, Version = 2
                        }));
                });

            var repo = new RefreshTokenRepo(store.Object, legacy.Object);
            var results = await Task.WhenAll(
                repo.TryConsumeRefreshTokenAsync(TokenId, UserId),
                repo.TryConsumeRefreshTokenAsync(TokenId, UserId));

            Assert.AreEqual(1, results.Count(result => result));
            store.Verify(x => x.TryReplaceAsync(
                It.Is<RefreshTokenOperationalRecord>(t => t.Consumed && t.Id == TokenId),
                1, It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [TestMethod]
        public async Task RevocationOfAlreadyConsumedTokenIsIdempotent()
        {
            var legacy = new Mock<ILegacyRefreshTokenReader>();
            var store = new Mock<IConditionalOperationalDataStore<RefreshTokenOperationalRecord>>();
            store.Setup(x => x.GetAsync(UserId, TokenId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RefreshTokenOperationalRecord { Id = TokenId, OrganizationId = UserId, Consumed = true, Version = 2 });

            var repo = new RefreshTokenRepo(store.Object, legacy.Object);
            await repo.RemoveRefreshTokenAsync(TokenId, UserId);

            store.Verify(x => x.TryReplaceAsync(
                It.IsAny<RefreshTokenOperationalRecord>(), It.IsAny<long>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        private static RefreshToken NewToken() =>
            new RefreshToken(UserId)
            {
                RowKey = TokenId,
                AppId = "APP",
                AppInstanceId = "INSTANCE",
                IssuedUtc = DateTime.UtcNow.ToJSONString(),
                ExpiresUtc = DateTime.UtcNow.AddDays(3).ToJSONString()
            };
    }
}
