using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Repos;
using OpenIddict.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.AuthorizationServer.Persistence.TableStorage
{
    /// <summary>
    /// OpenIddict 7.6 token store backed by the same Azure Table Storage account used by UserAdmin.
    ///
    /// The initial implementation deliberately keeps all protocol tokens in one partition. That makes
    /// the persistence semantics easy to prove first. Secondary lookup/index tables can be introduced
    /// once the cross-pod authorization-code flow is green.
    /// </summary>
    public class OpenIddictTableTokenStore : TableStorageBase<OpenIddictTableToken>, IOpenIddictTokenStore<OpenIddictTableToken>
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions();

        public OpenIddictTableTokenStore(IUserAdminSettings settings, IAdminLogger logger)
            : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
        }

        public async ValueTask<long> CountAsync(CancellationToken cancellationToken)
            => (await LoadAllAsync(cancellationToken)).LongCount();

        public async ValueTask<long> CountAsync<TResult>(Func<IQueryable<OpenIddictTableToken>, IQueryable<TResult>> query, CancellationToken cancellationToken)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            return query((await LoadAllAsync(cancellationToken)).AsQueryable()).LongCount();
        }

        public async ValueTask CreateAsync(OpenIddictTableToken token, CancellationToken cancellationToken)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            cancellationToken.ThrowIfCancellationRequested();

            if (String.IsNullOrWhiteSpace(token.Id))
                token.Id = Guid.NewGuid().ToString("N");

            token.PartitionKey = OpenIddictTableToken.StorePartitionKey;
            token.RowKey = OpenIddictTableToken.CreateRowKey(token.Id);
            await InsertAsync(token);
        }

        public async ValueTask DeleteAsync(OpenIddictTableToken token, CancellationToken cancellationToken)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            cancellationToken.ThrowIfCancellationRequested();
            EnsureKeys(token);
            await RemoveAsync(token, String.IsNullOrWhiteSpace(token.ETag) ? "*" : token.ETag);
        }

        public async IAsyncEnumerable<OpenIddictTableToken> FindAsync(
            string subject, string client, string status, string type,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (var token in await LoadAllAsync(cancellationToken))
            {
                if (Matches(token.Subject, subject) &&
                    Matches(token.ApplicationId, client) &&
                    Matches(token.Status, status) &&
                    Matches(token.Type, type))
                {
                    yield return token;
                }
            }
        }

        public IAsyncEnumerable<OpenIddictTableToken> FindByApplicationIdAsync(string identifier, CancellationToken cancellationToken)
            => FindByAsync(token => String.Equals(token.ApplicationId, identifier, StringComparison.Ordinal), cancellationToken);

        public IAsyncEnumerable<OpenIddictTableToken> FindByAuthorizationIdAsync(string identifier, CancellationToken cancellationToken)
            => FindByAsync(token => String.Equals(token.AuthorizationId, identifier, StringComparison.Ordinal), cancellationToken);

        public async ValueTask<OpenIddictTableToken> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(identifier)) throw new ArgumentNullException(nameof(identifier));
            cancellationToken.ThrowIfCancellationRequested();
            return await GetAsync(OpenIddictTableToken.StorePartitionKey, OpenIddictTableToken.CreateRowKey(identifier), false);
        }

        public async ValueTask<OpenIddictTableToken> FindByReferenceIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(identifier)) throw new ArgumentNullException(nameof(identifier));
            return (await LoadAllAsync(cancellationToken))
                .FirstOrDefault(token => String.Equals(token.ReferenceId, identifier, StringComparison.Ordinal));
        }

        public IAsyncEnumerable<OpenIddictTableToken> FindBySubjectAsync(string subject, CancellationToken cancellationToken)
            => FindByAsync(token => String.Equals(token.Subject, subject, StringComparison.Ordinal), cancellationToken);

        public ValueTask<string> GetApplicationIdAsync(OpenIddictTableToken token, CancellationToken cancellationToken)
            => FromValue(token, token.ApplicationId, cancellationToken);

        public async ValueTask<TResult> GetAsync<TState, TResult>(
            Func<IQueryable<OpenIddictTableToken>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            return query((await LoadAllAsync(cancellationToken)).AsQueryable(), state).FirstOrDefault();
        }

        public ValueTask<string> GetAuthorizationIdAsync(OpenIddictTableToken token, CancellationToken cancellationToken)
            => FromValue(token, token.AuthorizationId, cancellationToken);

        public ValueTask<DateTimeOffset?> GetCreationDateAsync(OpenIddictTableToken token, CancellationToken cancellationToken)
            => FromValue(token, ParseDate(token.CreationDateUtc), cancellationToken);

        public ValueTask<DateTimeOffset?> GetExpirationDateAsync(OpenIddictTableToken token, CancellationToken cancellationToken)
            => FromValue(token, ParseDate(token.ExpirationDateUtc), cancellationToken);

        public ValueTask<string> GetIdAsync(OpenIddictTableToken token, CancellationToken cancellationToken)
            => FromValue(token, token.Id, cancellationToken);

        public ValueTask<string> GetPayloadAsync(OpenIddictTableToken token, CancellationToken cancellationToken)
            => FromValue(token, token.Payload, cancellationToken);

        public ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(OpenIddictTableToken token, CancellationToken cancellationToken)
        {
            ValidateToken(token, cancellationToken);

            if (String.IsNullOrWhiteSpace(token.PropertiesJson))
                return new ValueTask<ImmutableDictionary<string, JsonElement>>(ImmutableDictionary<string, JsonElement>.Empty);

            var properties = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(token.PropertiesJson, _jsonOptions)
                ?? new Dictionary<string, JsonElement>();
            return new ValueTask<ImmutableDictionary<string, JsonElement>>(properties.ToImmutableDictionary(StringComparer.Ordinal));
        }

        public ValueTask<DateTimeOffset?> GetRedemptionDateAsync(OpenIddictTableToken token, CancellationToken cancellationToken)
            => FromValue(token, ParseDate(token.RedemptionDateUtc), cancellationToken);

        public ValueTask<string> GetReferenceIdAsync(OpenIddictTableToken token, CancellationToken cancellationToken)
            => FromValue(token, token.ReferenceId, cancellationToken);

        public ValueTask<string> GetStatusAsync(OpenIddictTableToken token, CancellationToken cancellationToken)
            => FromValue(token, token.Status, cancellationToken);

        public ValueTask<string> GetSubjectAsync(OpenIddictTableToken token, CancellationToken cancellationToken)
            => FromValue(token, token.Subject, cancellationToken);

        public ValueTask<string> GetTypeAsync(OpenIddictTableToken token, CancellationToken cancellationToken)
            => FromValue(token, token.Type, cancellationToken);

        public ValueTask<OpenIddictTableToken> InstantiateAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new ValueTask<OpenIddictTableToken>(new OpenIddictTableToken());
        }

        public async IAsyncEnumerable<OpenIddictTableToken> ListAsync(
            int? count, int? offset, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            IEnumerable<OpenIddictTableToken> tokens = await LoadAllAsync(cancellationToken);
            if (offset.HasValue) tokens = tokens.Skip(offset.Value);
            if (count.HasValue) tokens = tokens.Take(count.Value);

            foreach (var token in tokens)
                yield return token;
        }

        public async IAsyncEnumerable<TResult> ListAsync<TState, TResult>(
            Func<IQueryable<OpenIddictTableToken>, TState, IQueryable<TResult>> query,
            TState state, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            foreach (var item in query((await LoadAllAsync(cancellationToken)).AsQueryable(), state))
                yield return item;
        }

        public async ValueTask<long> PruneAsync(DateTimeOffset threshold, CancellationToken cancellationToken)
        {
            long count = 0;
            foreach (var token in await LoadAllAsync(cancellationToken))
            {
                var created = ParseDate(token.CreationDateUtc);
                if (!created.HasValue || created.Value >= threshold)
                    continue;

                if (String.Equals(token.Status, OpenIddictConstants.Statuses.Revoked, StringComparison.Ordinal) ||
                    String.Equals(token.Status, OpenIddictConstants.Statuses.Redeemed, StringComparison.Ordinal))
                {
                    await DeleteAsync(token, cancellationToken);
                    count++;
                }
            }

            return count;
        }

        public ValueTask<long> RevokeAsync(string subject, string client, string status, string type, CancellationToken cancellationToken)
            => RevokeWhereAsync(token =>
                Matches(token.Subject, subject) &&
                Matches(token.ApplicationId, client) &&
                Matches(token.Status, status) &&
                Matches(token.Type, type), cancellationToken);

        public ValueTask<long> RevokeByApplicationIdAsync(string identifier, CancellationToken cancellationToken = default)
            => RevokeWhereAsync(token => String.Equals(token.ApplicationId, identifier, StringComparison.Ordinal), cancellationToken);

        public ValueTask<long> RevokeByAuthorizationIdAsync(string identifier, CancellationToken cancellationToken)
            => RevokeWhereAsync(token => String.Equals(token.AuthorizationId, identifier, StringComparison.Ordinal), cancellationToken);

        public ValueTask<long> RevokeBySubjectAsync(string subject, CancellationToken cancellationToken = default)
            => RevokeWhereAsync(token => String.Equals(token.Subject, subject, StringComparison.Ordinal), cancellationToken);

        public ValueTask SetApplicationIdAsync(OpenIddictTableToken token, string identifier, CancellationToken cancellationToken)
            => SetValue(token, () => token.ApplicationId = identifier, cancellationToken);

        public ValueTask SetAuthorizationIdAsync(OpenIddictTableToken token, string identifier, CancellationToken cancellationToken)
            => SetValue(token, () => token.AuthorizationId = identifier, cancellationToken);

        public ValueTask SetCreationDateAsync(OpenIddictTableToken token, DateTimeOffset? date, CancellationToken cancellationToken)
            => SetValue(token, () => token.CreationDateUtc = FormatDate(date), cancellationToken);

        public ValueTask SetExpirationDateAsync(OpenIddictTableToken token, DateTimeOffset? date, CancellationToken cancellationToken)
            => SetValue(token, () => token.ExpirationDateUtc = FormatDate(date), cancellationToken);

        public ValueTask SetPayloadAsync(OpenIddictTableToken token, string payload, CancellationToken cancellationToken)
            => SetValue(token, () => token.Payload = payload, cancellationToken);

        public ValueTask SetPropertiesAsync(OpenIddictTableToken token, ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
            => SetValue(token, () => token.PropertiesJson = properties == null || properties.Count == 0
                ? null
                : JsonSerializer.Serialize(properties, _jsonOptions), cancellationToken);

        public ValueTask SetRedemptionDateAsync(OpenIddictTableToken token, DateTimeOffset? date, CancellationToken cancellationToken)
            => SetValue(token, () => token.RedemptionDateUtc = FormatDate(date), cancellationToken);

        public ValueTask SetReferenceIdAsync(OpenIddictTableToken token, string identifier, CancellationToken cancellationToken)
            => SetValue(token, () => token.ReferenceId = identifier, cancellationToken);

        public ValueTask SetStatusAsync(OpenIddictTableToken token, string status, CancellationToken cancellationToken)
            => SetValue(token, () => token.Status = status, cancellationToken);

        public ValueTask SetSubjectAsync(OpenIddictTableToken token, string subject, CancellationToken cancellationToken)
            => SetValue(token, () => token.Subject = subject, cancellationToken);

        public ValueTask SetTypeAsync(OpenIddictTableToken token, string type, CancellationToken cancellationToken)
            => SetValue(token, () => token.Type = type, cancellationToken);

        public async ValueTask UpdateAsync(OpenIddictTableToken token, CancellationToken cancellationToken)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            cancellationToken.ThrowIfCancellationRequested();
            EnsureKeys(token);

            // Existing protocol records must use their storage ETag. In particular, authorization-code
            // redemption is a state transition where only one competing pod may successfully update
            // a given token version. TableStorageBase converts a failed If-Match into ContentModifiedException.
            await base.UpdateAsync(token, String.IsNullOrWhiteSpace(token.ETag) ? "*" : token.ETag);
        }

        private async Task<IReadOnlyList<OpenIddictTableToken>> LoadAllAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tokens = await GetByPartitionIdAsync(OpenIddictTableToken.StorePartitionKey);
            cancellationToken.ThrowIfCancellationRequested();
            return tokens.ToList();
        }

        private async IAsyncEnumerable<OpenIddictTableToken> FindByAsync(
            Func<OpenIddictTableToken, bool> predicate,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            foreach (var token in await LoadAllAsync(cancellationToken))
            {
                if (predicate(token))
                    yield return token;
            }
        }

        private async ValueTask<long> RevokeWhereAsync(Func<OpenIddictTableToken, bool> predicate, CancellationToken cancellationToken)
        {
            long count = 0;
            foreach (var token in await LoadAllAsync(cancellationToken))
            {
                if (!predicate(token))
                    continue;

                token.Status = OpenIddictConstants.Statuses.Revoked;
                await UpdateAsync(token, cancellationToken);
                count++;
            }

            return count;
        }

        private static bool Matches(string actual, string requested)
            => requested == null || String.Equals(actual, requested, StringComparison.Ordinal);

        private static string FormatDate(DateTimeOffset? value)
            => value?.UtcDateTime.ToString("O", CultureInfo.InvariantCulture);

        private static DateTimeOffset? ParseDate(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return null;

            return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)
                ? parsed
                : (DateTimeOffset?)null;
        }

        private static void EnsureKeys(OpenIddictTableToken token)
        {
            if (String.IsNullOrWhiteSpace(token.Id))
                throw new InvalidOperationException("An OpenIddict token cannot be persisted without an identifier.");

            token.PartitionKey = OpenIddictTableToken.StorePartitionKey;
            token.RowKey = OpenIddictTableToken.CreateRowKey(token.Id);
        }

        private static void ValidateToken(OpenIddictTableToken token, CancellationToken cancellationToken)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            cancellationToken.ThrowIfCancellationRequested();
        }

        private static ValueTask<T> FromValue<T>(OpenIddictTableToken token, T value, CancellationToken cancellationToken)
        {
            ValidateToken(token, cancellationToken);
            return new ValueTask<T>(value);
        }

        private static ValueTask SetValue(OpenIddictTableToken token, Action setter, CancellationToken cancellationToken)
        {
            ValidateToken(token, cancellationToken);
            setter();
            return default;
        }
    }
}
