// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b33d78b016d4c2cfe92302fa913f25ab81632d81fd83dc7615751a405c1b4a87
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Security
{
    public class SecureLinkRepo :  TableStorageBase<SecureLink>, ISecureLinkRepo
    {
        public SecureLinkRepo(IUserAdminSettings settings, IAdminLogger logger) : 
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
            
        }

        public Task AddSecureLinkAsync(SecureLink secureLink)
        {
            return InsertAsync(secureLink);
        }

        public Task UpdateSecureLinkAsync(SecureLink secureLink)
        {
            return UpdateAsync(secureLink);
        }

        public Task<SecureLink> GetSecureLinkAsync(string orgId, string linkId)
        {
            return this.GetAsync(orgId, linkId);
        }

        public async Task RevokeLinkAsync(string orgId, string linkId)
        {
            var link = await GetSecureLinkAsync(orgId, linkId);
            link.Expired = true;
            await UpdateAsync(link);
        }
    }
}
