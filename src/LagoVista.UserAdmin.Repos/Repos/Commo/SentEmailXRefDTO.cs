using LagoVista.Core.Models;

namespace LagoVista.UserAdmin.Repos.Repos.Commo
{
    public class SentEmailXRefDTO : TableStorageEntity
    {
        public string SentEmailRowKey { get; set; }
    }

    public class SentEmailXRef
    {
        public string SentEmailRowKey { get; set; }
        public string OrgId { get; set; }
    }
}
