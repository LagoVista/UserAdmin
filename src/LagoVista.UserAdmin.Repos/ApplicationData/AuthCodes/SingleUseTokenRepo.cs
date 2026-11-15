using LagoVista.CloudStorage.Storage;
using LagoVista.Core;
using LagoVista.Core.Exceptions;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.ApplicationData.AuthCodes
{
    public sealed class SingleUseTokenRecord : IApplicationDataRecord
    {
        // Normal tokens last 60 seconds. A longer physical retention permits
        // explicitly configured token lifetimes and protects active grants.
        public static readonly TimeSpan Retention = TimeSpan.FromDays(30);

        public NormalizedId32 Id { get; set; }
        public EntityHeader Organization { get; set; }
        public UtcTimestamp CreationDate { get; set; }
        public UtcTimestamp LastUpdatedDate { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public string Expires { get; set; }
        public DateTime? ConsumedUtc { get; set; }

        public static NormalizedId32 CreateId(string userId, string token)
        {
            if (String.IsNullOrWhiteSpace(userId)) throw new ArgumentException("UserId is required.", nameof(userId));
            if (String.IsNullOrWhiteSpace(token)) throw new ArgumentException("Token is required.", nameof(token));
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes("useradmin:singleuse:v1:" + userId + ":" + token));
                return new NormalizedId32(BitConverter.ToString(bytes, 0, 16).Replace("-", String.Empty));
            }
        }
    }

    /// <summary>
    /// Single-use retrieval is an atomic compare-and-swap from unused to consumed.
    /// A racing second caller cannot receive a valid token.
    /// </summary>
    public sealed class SingleUseTokenRepo : ISingleUseTokenRepo
    {
        private readonly IApplicationDataStore _store;
        private readonly IAppConfig _config;

        public SingleUseTokenRepo(IApplicationDataStore store, IAppConfig config)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task StoreAsync(SingleUseToken token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            var record = new SingleUseTokenRecord
            {
                Id = SingleUseTokenRecord.CreateId(token.UserId, token.Token),
                Organization = RequireOrg(),
                UserId = token.UserId,
                Token = token.Token,
                Expires = token.Expires
            };
            await _store.InsertAsync(record).ConfigureAwait(false);
        }

        // Preserve the original public spelling for compatibility.
        public async Task<SingleUseToken> RetreiveAsync(string userId, string tokenId)
        {
            var id = SingleUseTokenRecord.CreateId(userId, tokenId);
            var read = await _store.GetVersionedAsync<SingleUseTokenRecord>(
                new StorageKey(id.Value, RequireOrg().Id)).ConfigureAwait(false);

            if (read?.Record == null || read.Record.ConsumedUtc.HasValue ||
                !String.Equals(read.Record.UserId, userId, StringComparison.Ordinal) ||
                !String.Equals(read.Record.Token, tokenId, StringComparison.Ordinal))
                return null;

            read.Record.ConsumedUtc = DateTime.UtcNow;
            try
            {
                await _store.UpdateIfVersionAsync(read.Record, read.ConcurrencyToken).ConfigureAwait(false);
            }
            catch (RecordConflictException)
            {
                return null;
            }
            catch (RecordNotFoundException)
            {
                return null;
            }

            return new SingleUseToken { UserId = userId, Token = tokenId, Expires = read.Record.Expires };
        }

        private EntityHeader RequireOrg()
        {
            var org = _config.SystemOwnerOrg;
            if (org == null || String.IsNullOrWhiteSpace(org.Id))
                throw new InvalidOperationException("SystemOwnerOrg is required for one-time token storage.");
            return org;
        }
    }
}
