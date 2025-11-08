// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 729848587413e9f30a21ef6c24de69b301e94fad2db4bc4e6623820d4b753a87
// IndexVersion: 2
// --- END CODE INDEX META ---
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
