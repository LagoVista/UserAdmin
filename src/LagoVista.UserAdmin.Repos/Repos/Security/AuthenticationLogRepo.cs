// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 905f78c6f3143d344e0f288b61412b8e27bb2ff0581d8f61231cf43160d47bf2
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Security
{
    public class AuthenticationLogRepo : IAuthenticationLogRepo
    {
        private const string CassandraCursorMarker = "cassandra";
        private readonly IActivityRecordStore<AuthenticationLog> _store;

        public AuthenticationLogRepo(IActivityRecordStore<AuthenticationLog> store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public static void ConfigureStorage(StorageDefinition<AuthenticationLog> definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));

            definition
                .PartitionBy(log => log.OrganizationId)
                .BucketBy(StoragePeriod.Month)
                .Index(log => log.UserId)
                .Index(log => log.UserName)
                .Index(log => log.AuthType);
        }

        public Task AddAsync(AuthenticationLog authLog)
        {
            PrepareForInsert(authLog);
            return _store.InsertAsync(authLog);
        }

        public Task<ListResponse<AuthenticationLog>> GetAllAsync(string organizationId, ListRequest listRequest)
        {
            return QueryAsync(organizationId, listRequest, null, null);
        }

        public Task<ListResponse<AuthenticationLog>> GetAsync(string organizationId, AuthLogTypes type, ListRequest listRequest)
        {
            return QueryAsync(organizationId, listRequest, nameof(AuthenticationLog.AuthType), type.ToString());
        }

        public Task<ListResponse<AuthenticationLog>> GetForUserIdAsync(string organizationId, string userId, ListRequest listRequest)
        {
            return QueryAsync(organizationId, listRequest, nameof(AuthenticationLog.UserId), userId);
        }

        public Task<ListResponse<AuthenticationLog>> GetForUserNameAsync(string organizationId, string userName, ListRequest listRequest)
        {
            return QueryAsync(organizationId, listRequest, nameof(AuthenticationLog.UserName), userName);
        }

        private async Task<ListResponse<AuthenticationLog>> QueryAsync(
            string organizationId,
            ListRequest listRequest,
            string indexedField,
            string indexedValue)
        {
            if (String.IsNullOrWhiteSpace(organizationId)) throw new ArgumentNullException(nameof(organizationId));
            if (listRequest == null) throw new ArgumentNullException(nameof(listRequest));

            if (!listRequest.TryGetDateRange(out var start, out var endExclusive, out var error))
            {
                return ListResponse<AuthenticationLog>.FromError(error);
            }

            if (!start.HasValue || !endExclusive.HasValue)
            {
                return ListResponse<AuthenticationLog>.FromError(
                    "Authentication log queries require both StartDate and EndDate because the Cassandra activity store is month bucketed.");
            }

            string continuationToken = null;
            if (listRequest.HasCursor)
            {
                if (!String.Equals(listRequest.NextPartitionKey, CassandraCursorMarker, StringComparison.Ordinal))
                {
                    return ListResponse<AuthenticationLog>.FromError("The authentication log continuation cursor is not a Cassandra cursor.");
                }

                continuationToken = listRequest.NextRowKey;
            }

            var pageSize = listRequest.PageSize <= 0 ? 100 : Math.Min(listRequest.PageSize, 1000);
            var query = new HistoryQuery<AuthenticationLog>()
                .Where(log => log.OrganizationId, StorageFilterOperator.Equal, organizationId)
                .Between(start.Value, endExclusive.Value.AddTicks(-1))
                .WithPage(new StoragePageRequest(pageSize, continuationToken));

            if (!String.IsNullOrWhiteSpace(indexedField))
            {
                if (String.Equals(indexedField, nameof(AuthenticationLog.UserId), StringComparison.Ordinal))
                {
                    query.Where(log => log.UserId, StorageFilterOperator.Equal, indexedValue);
                }
                else if (String.Equals(indexedField, nameof(AuthenticationLog.UserName), StringComparison.Ordinal))
                {
                    query.Where(log => log.UserName, StorageFilterOperator.Equal, indexedValue);
                }
                else if (String.Equals(indexedField, nameof(AuthenticationLog.AuthType), StringComparison.Ordinal))
                {
                    query.Where(log => log.AuthType, StorageFilterOperator.Equal, indexedValue);
                }
                else
                {
                    throw new NotSupportedException($"Authentication log index {indexedField} is not registered.");
                }
            }

            var result = await _store.QueryAsync(query).ConfigureAwait(false);
            return ListResponse<AuthenticationLog>.Create(
                result.Items,
                listRequest,
                result.HasMoreRecords,
                result.HasMoreRecords ? CassandraCursorMarker : null,
                result.ContinuationToken);
        }

        private static void PrepareForInsert(AuthenticationLog authLog)
        {
            if (authLog == null) throw new ArgumentNullException(nameof(authLog));
            if (String.IsNullOrWhiteSpace(authLog.OrganizationId))
            {
                throw new InvalidOperationException("AuthenticationLog requires OrganizationId before it can be persisted.");
            }

            if (String.IsNullOrWhiteSpace(authLog.Id)) authLog.Id = Guid.NewGuid().ToString("N");
            if (authLog.CreationDate == default) authLog.CreationDate = DateTime.UtcNow;
        }
    }
}
