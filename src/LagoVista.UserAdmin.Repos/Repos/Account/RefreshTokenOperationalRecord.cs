using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Authentication.Models;
using System;

namespace LagoVista.UserAdmin.Repos.Users
{
    /// <summary>
    /// Short-lived refresh-token state keyed by user id and the existing token id.
    /// Keep consumed rows as tombstones so an old Azure row cannot be re-imported.
    /// </summary>
    public sealed class RefreshTokenOperationalRecord : IConditionalOperationalDataRecord
    {
        public string OrganizationId { get; set; }
        public string Id { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public long Version { get; set; }
        public string AppId { get; set; }
        public string AppInstanceId { get; set; }
        public string IssuedUtc { get; set; }
        public string ExpiresUtc { get; set; }
        public bool Consumed { get; set; }

        public static RefreshTokenOperationalRecord FromToken(RefreshToken token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            return new RefreshTokenOperationalRecord
            {
                OrganizationId = token.PartitionKey,
                Id = token.RowKey,
                AppId = token.AppId,
                AppInstanceId = token.AppInstanceId,
                IssuedUtc = token.IssuedUtc,
                ExpiresUtc = token.ExpiresUtc
            };
        }

        public RefreshToken ToToken()
        {
            return new RefreshToken(OrganizationId)
            {
                RowKey = Id,
                AppId = AppId,
                AppInstanceId = AppInstanceId,
                IssuedUtc = IssuedUtc,
                ExpiresUtc = ExpiresUtc
            };
        }
    }
}
