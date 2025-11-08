// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 6ea728ebf2e71e073d4bd0bd8d252f08b6f56e733042841b3ae38e25e6f71fae
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Commo
{
    public class SentEmailXRefRepo : TableStorageBase<SentEmailXRefDTO>
    {
        public SentEmailXRefRepo(IUserAdminSettings settings, IAdminLogger logger) :
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {

        }
 
        public async Task AddAsync(string externalMessageId, string orgId, string sentEmailRowKey)
        {
            var dto = new SentEmailXRefDTO()
            {
                RowKey = externalMessageId,
                PartitionKey = orgId,
                SentEmailRowKey = sentEmailRowKey
            };

            await InsertAsync(dto);
        }

        public async Task<SentEmailXRef> ResolveAsync(string externalMessageId)
        {

            var record = await GetAsync(externalMessageId, false);
            if (record == null)
                return null;

            return new SentEmailXRef()
            {
                 OrgId = record.PartitionKey,
                 SentEmailRowKey = record.RowKey
            };
        }
    }
}
