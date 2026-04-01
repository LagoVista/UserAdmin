namespace LagoVista.AspNetCore.Identity.Interfaces
{
    public interface IDataProtectionSettings
    {
        public string KeyVaultKeyId { get; }
        public string StorageAccountName { get; }
        public string StorageAccountKey { get; }
        public string StorageKeyContainerName { get; }
        public string StorageKeyBlobName { get; }
        public string TenantId { get; }
        public string KVAppId { get; }
        public string KVAppSecret { get; }
    }
}
