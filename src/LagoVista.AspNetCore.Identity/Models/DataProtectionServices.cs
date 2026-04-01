using LagoVista.AspNetCore.Identity.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LagoVista.AspNetCore.Identity.Models
{
    public class DataProtectionSettings : IDataProtectionSettings
    {
        public DataProtectionSettings(IConfiguration config)
        {
            var section = config.GetSection("DataProtection");
            KeyVaultKeyId = section.Require("KeyVaultKeyId");
            StorageAccountName = section.Require("StorageAccountName");
            StorageAccountKey = section.Require("StorageAccountKey");
            StorageKeyContainerName = section.Require("StorageKeyContainerName");
            StorageKeyBlobName = section.Require("StorageKeyBlobName");
            TenantId = section.Require("TenantId");
            KVAppId = section.Require("KVAppId");
            KVAppSecret = section.Require("KVAppSecret");
        }

        public string KeyVaultKeyId { get; private set; }
        public string StorageAccountName { get; private set; }
        public string StorageAccountKey { get; private set; }
        public string StorageKeyContainerName { get; private set; }
        public string StorageKeyBlobName { get; private set; }
        public string TenantId { get; private set; }
        public string KVAppId { get; private set; }
        public string KVAppSecret { get; private set; }
    }
}
