using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models;
using System;

namespace LagoVista.UserAdmin.Repos.TableStorage.ProvisionalEnvironments
{
    internal class ProvisionalEnvironmentInstallationIndexEntity : TableStorageEntity
    {
        public string InstallationIdHash { get; set; }
        public string EnvironmentId { get; set; }
        public string CreatedUtc { get; set; }

        public static string CreatePartitionKey(string installationIdHash)
        {
            if (String.IsNullOrEmpty(installationIdHash)) throw new ArgumentNullException(nameof(installationIdHash));

            var prefixLength = Math.Min(2, installationIdHash.Length);
            return $"INS|{installationIdHash.Substring(0, prefixLength).ToLowerInvariant()}";
        }

        public static string CreateRowKey(string installationIdHash)
        {
            if (String.IsNullOrEmpty(installationIdHash)) throw new ArgumentNullException(nameof(installationIdHash));
            return $"INS|{installationIdHash}";
        }
    }
}
