using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models;
using System;

namespace LagoVista.UserAdmin.Repos.TableStorage.ProvisionalEnvironments
{
    internal class ProvisionalEnvironmentRecoveryIndexEntity : TableStorageEntity
    {
        public string RecoveryTokenHash { get; set; }
        public string EnvironmentId { get; set; }
        public string CreatedUtc { get; set; }

        public static string CreatePartitionKey(string recoveryTokenHash)
        {
            if (String.IsNullOrEmpty(recoveryTokenHash)) throw new ArgumentNullException(nameof(recoveryTokenHash));

            var prefixLength = Math.Min(2, recoveryTokenHash.Length);
            return $"REC|{recoveryTokenHash.Substring(0, prefixLength).ToLowerInvariant()}";
        }

        public static string CreateRowKey(string recoveryTokenHash)
        {
            if (String.IsNullOrEmpty(recoveryTokenHash)) throw new ArgumentNullException(nameof(recoveryTokenHash));
            return $"REC|{recoveryTokenHash}";
        }
    }
}
