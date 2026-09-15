using Cassandra;
using LagoVista.CloudStorage.Exceptions;
using LagoVista.CloudStorage.Storage.StorageProviders.Cassandra;
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

namespace LagoVista.AspNetCore.AuthorizationServer.Persistence.Cassandra
{
    /// <summary>
    /// Cassandra-backed OpenIddict protocol-token store. Records are short-lived and written with
    /// per-row TTL. Reference-id lookups use a Cassandra SAI index rather than scanning protocol state.
    /// Optimistic updates use a monotonically increasing version and LWT so authorization-code
    /// redemption remains single-use across authorization-server replicas.
    /// </summary>
    public sealed class OpenIddictCassandraTokenStore : IOpenIddictTokenStore<OpenIddictProtocolToken>
    {
        private const string TableName = "openiddict_protocol_tokens";
        private const string SelectColumns = "id, application_id, authorization_id, subject, type, status, reference_id, payload, properties_json, creation_date_utc, expiration_date_utc, redemption_date_utc, version";
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions();

        private readonly ICassandraSessionFactory _sessionFactory;
        private readonly AuthorizationServerOptions _options;
        private readonly SemaphoreSlim _schemaLock = new SemaphoreSlim(1, 1);
        private volatile bool _schemaReady;

        public OpenIddictCassandraTokenStore(ICassandraSessionFactory sessionFactory, AuthorizationServerOptions options)
        {
            _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async ValueTask<long> CountAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var session = await GetReadySessionAsync().ConfigureAwait(false);
            var rows = await session.ExecuteAsync(new SimpleStatement($"SELECT count(*) FROM {TableName}")).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return rows.First().GetValue<long>(0);
        }

        public async ValueTask<long> CountAsync<TResult>(Func<IQueryable<OpenIddictProtocolToken>, IQueryable<TResult>> query, CancellationToken cancellationToken)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            return query((await LoadAllAsync(cancellationToken).ConfigureAwait(false)).AsQueryable()).LongCount();
        }

        public async ValueTask CreateAsync(OpenIddictProtocolToken token, CancellationToken cancellationToken)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            cancellationToken.ThrowIfCancellationRequested();

            if (String.IsNullOrWhiteSpace(token.Id))
                token.Id = Guid.NewGuid().ToString("N");

            token.Version = 0;
            var session = await GetReadySessionAsync().ConfigureAwait(false);
            var prepared = await session.PrepareAsync($@"
INSERT INTO {TableName}
(id, application_id, authorization_id, subject, type, status, reference_id, payload, properties_json, creation_date_utc, expiration_date_utc, redemption_date_utc, version)
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
USING TTL ?").ConfigureAwait(false);

            await session.ExecuteAsync(prepared.Bind(
                token.Id,
                token.ApplicationId,
                token.AuthorizationId,
                token.Subject,
                token.Type,
                token.Status,
                token.ReferenceId,
                token.Payload,
                token.PropertiesJson,
                ToTimestamp(token.CreationDateUtc),
                ToTimestamp(token.ExpirationDateUtc),
                ToTimestamp(token.RedemptionDateUtc),
                token.Version,
                GetTtlSeconds(token))).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
        }

        public async ValueTask DeleteAsync(OpenIddictProtocolToken token, CancellationToken cancellationToken)
        {
            ValidatePersistedToken(token, cancellationToken);
            var session = await GetReadySessionAsync().ConfigureAwait(false);
            var prepared = await session.PrepareAsync($"DELETE FROM {TableName} WHERE id = ?").ConfigureAwait(false);
            await session.ExecuteAsync(prepared.Bind(token.Id)).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
        }

        public IAsyncEnumerable<OpenIddictProtocolToken> FindAsync(string subject, string client, string status, string type, CancellationToken cancellationToken)
            => QueryAsync(subject, client, status, type, cancellationToken);

        public IAsyncEnumerable<OpenIddictProtocolToken> FindByApplicationIdAsync(string identifier, CancellationToken cancellationToken)
            => QueryColumnAsync("application_id", identifier, cancellationToken);

        public IAsyncEnumerable<OpenIddictProtocolToken> FindByAuthorizationIdAsync(string identifier, CancellationToken cancellationToken)
            => QueryColumnAsync("authorization_id", identifier, cancellationToken);

        public async ValueTask<OpenIddictProtocolToken> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(identifier)) throw new ArgumentNullException(nameof(identifier));
            cancellationToken.ThrowIfCancellationRequested();

            var session = await GetReadySessionAsync().ConfigureAwait(false);
            var prepared = await session.PrepareAsync($"SELECT {SelectColumns} FROM {TableName} WHERE id = ?").ConfigureAwait(false);
            var rows = await session.ExecuteAsync(prepared.Bind(identifier)).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return ReadToken(rows.FirstOrDefault());
        }

        public async ValueTask<OpenIddictProtocolToken> FindByReferenceIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(identifier)) throw new ArgumentNullException(nameof(identifier));
            cancellationToken.ThrowIfCancellationRequested();

            var session = await GetReadySessionAsync().ConfigureAwait(false);
            var prepared = await session.PrepareAsync($"SELECT {SelectColumns} FROM {TableName} WHERE reference_id = ? LIMIT 1").ConfigureAwait(false);
            var rows = await session.ExecuteAsync(prepared.Bind(identifier)).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return ReadToken(rows.FirstOrDefault());
        }

        public IAsyncEnumerable<OpenIddictProtocolToken> FindBySubjectAsync(string subject, CancellationToken cancellationToken)
            => QueryColumnAsync("subject", subject, cancellationToken);

        public ValueTask<string> GetApplicationIdAsync(OpenIddictProtocolToken token, CancellationToken cancellationToken)
            => FromValue(token, token.ApplicationId, cancellationToken);

        public async ValueTask<TResult> GetAsync<TState, TResult>(Func<IQueryable<OpenIddictProtocolToken>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            return query((await LoadAllAsync(cancellationToken).ConfigureAwait(false)).AsQueryable(), state).FirstOrDefault();
        }

        public ValueTask<string> GetAuthorizationIdAsync(OpenIddictProtocolToken token, CancellationToken cancellationToken)
            => FromValue(token, token.AuthorizationId, cancellationToken);

        public ValueTask<DateTimeOffset?> GetCreationDateAsync(OpenIddictProtocolToken token, CancellationToken cancellationToken)
            => FromValue(token, ParseDate(token.CreationDateUtc), cancellationToken);

        public ValueTask<DateTimeOffset?> GetExpirationDateAsync(OpenIddictProtocolToken token, CancellationToken cancellationToken)
            => FromValue(token, ParseDate(token.ExpirationDateUtc), cancellationToken);

        public ValueTask<string> GetIdAsync(OpenIddictProtocolToken token, CancellationToken cancellationToken)
            => FromValue(token, token.Id, cancellationToken);

        public ValueTask<string> GetPayloadAsync(OpenIddictProtocolToken token, CancellationToken cancellationToken)
            => FromValue(token, token.Payload, cancellationToken);

        public ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(OpenIddictProtocolToken token, CancellationToken cancellationToken)
        {
            ValidateTokenInstance(token, cancellationToken);
            if (String.IsNullOrWhiteSpace(token.PropertiesJson))
                return new ValueTask<ImmutableDictionary<string, JsonElement>>(ImmutableDictionary<string, JsonElement>.Empty);

            var properties = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(token.PropertiesJson, _jsonOptions)
                ?? new Dictionary<string, JsonElement>();
            return new ValueTask<ImmutableDictionary<string, JsonElement>>(properties.ToImmutableDictionary(StringComparer.Ordinal));
        }

        public ValueTask<DateTimeOffset?> GetRedemptionDateAsync(OpenIddictProtocolToken token, CancellationToken cancellationToken)
            => FromValue(token, ParseDate(token.RedemptionDateUtc), cancellationToken);

        public ValueTask<string> GetReferenceIdAsync(OpenIddictProtocolToken token, CancellationToken cancellationToken)
            => FromValue(token, token.ReferenceId, cancellationToken);

        public ValueTask<string> GetStatusAsync(OpenIddictProtocolToken token, CancellationToken cancellationToken)
            => FromValue(token, token.Status, cancellationToken);

        public ValueTask<string> GetSubjectAsync(OpenIddictProtocolToken token, CancellationToken cancellationToken)
            => FromValue(token, token.Subject, cancellationToken);

        public ValueTask<string> GetTypeAsync(OpenIddictProtocolToken token, CancellationToken cancellationToken)
            => FromValue(token, token.Type, cancellationToken);

        public ValueTask<OpenIddictProtocolToken> InstantiateAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new ValueTask<OpenIddictProtocolToken>(new OpenIddictProtocolToken());
        }

        public async IAsyncEnumerable<OpenIddictProtocolToken> ListAsync(int? count, int? offset, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            IEnumerable<OpenIddictProtocolToken> tokens = await LoadAllAsync(cancellationToken).ConfigureAwait(false);
            if (offset.HasValue) tokens = tokens.Skip(offset.Value);
            if (count.HasValue) tokens = tokens.Take(count.Value);

            foreach (var token in tokens)
                yield return token;
        }

        public async IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<OpenIddictProtocolToken>, TState, IQueryable<TResult>> query, TState state, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            foreach (var item in query((await LoadAllAsync(cancellationToken).ConfigureAwait(false)).AsQueryable(), state))
                yield return item;
        }

        public async ValueTask<long> PruneAsync(DateTimeOffset threshold, CancellationToken cancellationToken)
        {
            long count = 0;
            foreach (var token in await LoadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                var created = ParseDate(token.CreationDateUtc);
                if (!created.HasValue || created.Value >= threshold) continue;

                if (String.Equals(token.Status, OpenIddictConstants.Statuses.Revoked, StringComparison.Ordinal) ||
                    String.Equals(token.Status, OpenIddictConstants.Statuses.Redeemed, StringComparison.Ordinal))
                {
                    await DeleteAsync(token, cancellationToken).ConfigureAwait(false);
                    count++;
                }
            }

            return count;
        }

        public ValueTask<long> RevokeAsync(string subject, string client, string status, string type, CancellationToken cancellationToken)
            => RevokeWhereAsync(token => Matches(token.Subject, subject) && Matches(token.ApplicationId, client) && Matches(token.Status, status) && Matches(token.Type, type), cancellationToken);

        public ValueTask<long> RevokeByApplicationIdAsync(string identifier, CancellationToken cancellationToken = default)
            => RevokeWhereAsync(token => String.Equals(token.ApplicationId, identifier, StringComparison.Ordinal), cancellationToken);

        public ValueTask<long> RevokeByAuthorizationIdAsync(string identifier, CancellationToken cancellationToken)
            => RevokeWhereAsync(token => String.Equals(token.AuthorizationId, identifier, StringComparison.Ordinal), cancellationToken);

        public ValueTask<long> RevokeBySubjectAsync(string subject, CancellationToken cancellationToken = default)
            => RevokeWhereAsync(token => String.Equals(token.Subject, subject, StringComparison.Ordinal), cancellationToken);

        public ValueTask SetApplicationIdAsync(OpenIddictProtocolToken token, string identifier, CancellationToken cancellationToken)
            => SetValue(token, () => token.ApplicationId = identifier, cancellationToken);

        public ValueTask SetAuthorizationIdAsync(OpenIddictProtocolToken token, string identifier, CancellationToken cancellationToken)
            => SetValue(token, () => token.AuthorizationId = identifier, cancellationToken);

        public ValueTask SetCreationDateAsync(OpenIddictProtocolToken token, DateTimeOffset? date, CancellationToken cancellationToken)
            => SetValue(token, () => token.CreationDateUtc = FormatDate(date), cancellationToken);

        public ValueTask SetExpirationDateAsync(OpenIddictProtocolToken token, DateTimeOffset? date, CancellationToken cancellationToken)
            => SetValue(token, () => token.ExpirationDateUtc = FormatDate(date), cancellationToken);

        public ValueTask SetPayloadAsync(OpenIddictProtocolToken token, string payload, CancellationToken cancellationToken)
            => SetValue(token, () => token.Payload = payload, cancellationToken);

        public ValueTask SetPropertiesAsync(OpenIddictProtocolToken token, ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
            => SetValue(token, () => token.PropertiesJson = properties == null || properties.Count == 0 ? null : JsonSerializer.Serialize(properties, _jsonOptions), cancellationToken);

        public ValueTask SetRedemptionDateAsync(OpenIddictProtocolToken token, DateTimeOffset? date, CancellationToken cancellationToken)
            => SetValue(token, () => token.RedemptionDateUtc = FormatDate(date), cancellationToken);

        public ValueTask SetReferenceIdAsync(OpenIddictProtocolToken token, string identifier, CancellationToken cancellationToken)
            => SetValue(token, () => token.ReferenceId = identifier, cancellationToken);

        public ValueTask SetStatusAsync(OpenIddictProtocolToken token, string status, CancellationToken cancellationToken)
            => SetValue(token, () => token.Status = status, cancellationToken);

        public ValueTask SetSubjectAsync(OpenIddictProtocolToken token, string subject, CancellationToken cancellationToken)
            => SetValue(token, () => token.Subject = subject, cancellationToken);

        public ValueTask SetTypeAsync(OpenIddictProtocolToken token, string type, CancellationToken cancellationToken)
            => SetValue(token, () => token.Type = type, cancellationToken);

        public async ValueTask UpdateAsync(OpenIddictProtocolToken token, CancellationToken cancellationToken)
        {
            ValidatePersistedToken(token, cancellationToken);
            var expectedVersion = token.Version;
            var nextVersion = expectedVersion + 1;
            var session = await GetReadySessionAsync().ConfigureAwait(false);
            var prepared = await session.PrepareAsync($@"
UPDATE {TableName} USING TTL ? SET
application_id = ?, authorization_id = ?, subject = ?, type = ?, status = ?, reference_id = ?, payload = ?, properties_json = ?,
creation_date_utc = ?, expiration_date_utc = ?, redemption_date_utc = ?, version = ?
WHERE id = ? IF version = ?").ConfigureAwait(false);

            var rows = await session.ExecuteAsync(prepared.Bind(
                GetTtlSeconds(token),
                token.ApplicationId,
                token.AuthorizationId,
                token.Subject,
                token.Type,
                token.Status,
                token.ReferenceId,
                token.Payload,
                token.PropertiesJson,
                ToTimestamp(token.CreationDateUtc),
                ToTimestamp(token.ExpirationDateUtc),
                ToTimestamp(token.RedemptionDateUtc),
                nextVersion,
                token.Id,
                expectedVersion)).ConfigureAwait(false);

            var result = rows.FirstOrDefault();
            if (result == null || !result.GetValue<bool>("[applied]"))
                throw new ContentModifiedException();

            token.Version = nextVersion;
            cancellationToken.ThrowIfCancellationRequested();
        }

        private async Task<ISession> GetReadySessionAsync()
        {
            var session = await _sessionFactory.GetSessionAsync().ConfigureAwait(false);
            if (_schemaReady) return session;

            await _schemaLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (!_schemaReady)
                {
                    await session.ExecuteAsync(new SimpleStatement($@"
CREATE TABLE IF NOT EXISTS {TableName} (
    id text PRIMARY KEY,
    application_id text,
    authorization_id text,
    subject text,
    type text,
    status text,
    reference_id text,
    payload text,
    properties_json text,
    creation_date_utc timestamp,
    expiration_date_utc timestamp,
    redemption_date_utc timestamp,
    version bigint
)")).ConfigureAwait(false);

                    foreach (var column in new[] { "reference_id", "application_id", "authorization_id", "subject", "status", "type" })
                    {
                        await session.ExecuteAsync(new SimpleStatement(
                            $"CREATE INDEX IF NOT EXISTS {TableName}_{column}_idx ON {TableName} ({column}) USING 'sai'"))
                            .ConfigureAwait(false);
                    }

                    _schemaReady = true;
                }
            }
            finally
            {
                _schemaLock.Release();
            }

            return session;
        }

        private async Task<IReadOnlyList<OpenIddictProtocolToken>> LoadAllAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var session = await GetReadySessionAsync().ConfigureAwait(false);
            var rows = await session.ExecuteAsync(new SimpleStatement($"SELECT {SelectColumns} FROM {TableName}")).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return rows.Select(ReadToken).Where(token => token != null).ToList();
        }

        private async IAsyncEnumerable<OpenIddictProtocolToken> QueryColumnAsync(string column, string value, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(value)) yield break;
            var session = await GetReadySessionAsync().ConfigureAwait(false);
            var prepared = await session.PrepareAsync($"SELECT {SelectColumns} FROM {TableName} WHERE {column} = ?").ConfigureAwait(false);
            var rows = await session.ExecuteAsync(prepared.Bind(value)).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var row in rows)
                yield return ReadToken(row);
        }

        private async IAsyncEnumerable<OpenIddictProtocolToken> QueryAsync(string subject, string client, string status, string type, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var filters = new List<(string Column, string Value)>();
            if (!String.IsNullOrWhiteSpace(subject)) filters.Add(("subject", subject));
            if (!String.IsNullOrWhiteSpace(client)) filters.Add(("application_id", client));
            if (!String.IsNullOrWhiteSpace(status)) filters.Add(("status", status));
            if (!String.IsNullOrWhiteSpace(type)) filters.Add(("type", type));

            if (filters.Count == 0)
            {
                foreach (var token in await LoadAllAsync(cancellationToken).ConfigureAwait(false))
                    yield return token;
                yield break;
            }

            var session = await GetReadySessionAsync().ConfigureAwait(false);
            var cql = $"SELECT {SelectColumns} FROM {TableName} WHERE {String.Join(" AND ", filters.Select(filter => $"{filter.Column} = ?"))}";
            var prepared = await session.PrepareAsync(cql).ConfigureAwait(false);
            var rows = await session.ExecuteAsync(prepared.Bind(filters.Select(filter => (object)filter.Value).ToArray())).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var row in rows)
                yield return ReadToken(row);
        }

        private async ValueTask<long> RevokeWhereAsync(Func<OpenIddictProtocolToken, bool> predicate, CancellationToken cancellationToken)
        {
            long count = 0;
            foreach (var token in await LoadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                if (!predicate(token)) continue;
                token.Status = OpenIddictConstants.Statuses.Revoked;
                await UpdateAsync(token, cancellationToken).ConfigureAwait(false);
                count++;
            }
            return count;
        }

        private int GetTtlSeconds(OpenIddictProtocolToken token)
        {
            var retentionSeconds = Math.Max(60, (int)Math.Ceiling(_options.ProtocolTokenRetention.TotalSeconds));
            var expiration = ParseDate(token.ExpirationDateUtc);
            if (!expiration.HasValue) return retentionSeconds;

            var untilExpirationAndGrace = expiration.Value - DateTimeOffset.UtcNow + _options.ProtocolTokenRetention;
            return Math.Max(retentionSeconds, (int)Math.Ceiling(untilExpirationAndGrace.TotalSeconds));
        }

        private static OpenIddictProtocolToken ReadToken(Row row)
        {
            if (row == null) return null;
            var id = row.GetValue<string>("id");
            return new OpenIddictProtocolToken
            {
                Id = id,
                ApplicationId = GetString(row, "application_id"),
                AuthorizationId = GetString(row, "authorization_id"),
                Subject = GetString(row, "subject"),
                Type = GetString(row, "type"),
                Status = GetString(row, "status"),
                ReferenceId = GetString(row, "reference_id"),
                Payload = GetString(row, "payload"),
                PropertiesJson = GetString(row, "properties_json"),
                CreationDateUtc = FormatDate(GetTimestamp(row, "creation_date_utc")),
                ExpirationDateUtc = FormatDate(GetTimestamp(row, "expiration_date_utc")),
                RedemptionDateUtc = FormatDate(GetTimestamp(row, "redemption_date_utc")),
                Version = row.IsNull("version") ? 0 : row.GetValue<long>("version")
            };
        }

        private static string GetString(Row row, string column)
            => row.IsNull(column) ? null : row.GetValue<string>(column);

        private static DateTimeOffset? GetTimestamp(Row row, string column)
            => row.IsNull(column) ? (DateTimeOffset?)null : row.GetValue<DateTimeOffset>(column);

        private static object ToTimestamp(string value)
            => ParseDate(value).HasValue ? (object)ParseDate(value).Value : null;

        private static bool Matches(string actual, string requested)
            => requested == null || String.Equals(actual, requested, StringComparison.Ordinal);

        private static string FormatDate(DateTimeOffset? value)
            => value?.UtcDateTime.ToString("O", CultureInfo.InvariantCulture);

        private static DateTimeOffset? ParseDate(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return null;
            return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)
                ? parsed
                : (DateTimeOffset?)null;
        }

        private static void ValidateTokenInstance(OpenIddictProtocolToken token, CancellationToken cancellationToken)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            cancellationToken.ThrowIfCancellationRequested();
        }

        private static void ValidatePersistedToken(OpenIddictProtocolToken token, CancellationToken cancellationToken)
        {
            ValidateTokenInstance(token, cancellationToken);
            if (String.IsNullOrWhiteSpace(token.Id)) throw new InvalidOperationException("An OpenIddict token cannot be persisted without an identifier.");
        }

        private static ValueTask<T> FromValue<T>(OpenIddictProtocolToken token, T value, CancellationToken cancellationToken)
        {
            ValidateTokenInstance(token, cancellationToken);
            return new ValueTask<T>(value);
        }

        private static ValueTask SetValue(OpenIddictProtocolToken token, Action setter, CancellationToken cancellationToken)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            cancellationToken.ThrowIfCancellationRequested();
            setter();
            return default;
        }
    }
}
