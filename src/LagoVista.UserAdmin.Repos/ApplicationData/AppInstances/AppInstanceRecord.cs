using LagoVista.CloudStorage.Storage;
using LagoVista.Core;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Apps;
using System;
using System.Security.Cryptography;
using System.Text;

namespace LagoVista.UserAdmin.Repos.ApplicationData.AppInstances
{
    /// <summary>Durable Mongo record preserving legacy application-instance identity and fields.</summary>
    public sealed class AppInstanceRecord : IApplicationDataRecord
    {
        public NormalizedId32 Id { get; set; }
        public EntityHeader Organization { get; set; }
        public UtcTimestamp CreationDate { get; set; }
        public UtcTimestamp LastUpdatedDate { get; set; }

        public string UserPartition { get; set; }
        public string InstanceKey { get; set; }
        public string StoredUserId { get; set; }
        public string AppId { get; set; }
        public string ClientType { get; set; }
        public string DeviceId { get; set; }
        public string OriginalCreationDate { get; set; }
        public string LastAccessTokenRefresh { get; set; }
        public string LastLogin { get; set; }

        public static NormalizedId32 CreateId(string userId, string instanceId)
        {
            if (String.IsNullOrWhiteSpace(userId)) throw new ArgumentException("UserId required.", nameof(userId));
            if (String.IsNullOrWhiteSpace(instanceId)) throw new ArgumentException("AppInstanceId required.", nameof(instanceId));
            using (var sha = SHA256.Create())
            {
                var source = "useradmin-appinstance:v1:" + userId.Length + ":" + userId + ":" + instanceId;
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(source));
                return new NormalizedId32(BitConverter.ToString(hash, 0, 16).Replace("-", String.Empty));
            }
        }

        public static AppInstanceRecord FromModel(AppInstance model, EntityHeader platformOrg)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (platformOrg == null || String.IsNullOrWhiteSpace(platformOrg.Id))
                throw new InvalidOperationException("SystemOwnerOrg is required for AppInstance storage.");
            var userId = model.PartitionKey;
            var instanceId = model.RowKey;
            var id = CreateId(userId, instanceId);
            if (!String.IsNullOrWhiteSpace(model.UserId) &&
                !String.Equals(model.UserId, userId, StringComparison.Ordinal))
                throw new ArgumentException("AppInstance.UserId must match its PartitionKey.", nameof(model));

            return new AppInstanceRecord
            {
                Id = id,
                Organization = platformOrg,
                UserPartition = userId,
                InstanceKey = instanceId,
                StoredUserId = model.UserId,
                AppId = model.AppId,
                ClientType = model.ClientType,
                DeviceId = model.DeviceId,
                OriginalCreationDate = model.CreationDate,
                LastAccessTokenRefresh = model.LastAccessTokenRefresh,
                LastLogin = model.LastLogin
            };
        }

        public AppInstance ToModel()
        {
            return new AppInstance(InstanceKey, UserPartition)
            {
                UserId = StoredUserId,
                AppId = AppId,
                ClientType = ClientType,
                DeviceId = DeviceId,
                CreationDate = OriginalCreationDate,
                LastAccessTokenRefresh = LastAccessTokenRefresh,
                LastLogin = LastLogin
            };
        }
    }
}
