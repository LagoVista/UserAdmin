// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e83be0e0bfb63b208af22c57c84d240b7d40d5b4a2bcbf451af1db2719255e05
// IndexVersion: 2
// --- END CODE INDEX META ---
using Azure.Storage.Blobs;
using LagoVista.CloudStorage.Storage;
using LagoVista.Core;
using LagoVista.Core.Exceptions;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Commo;
using LagoVista.UserAdmin.Models.Commo;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Commo
{
    public class SentEmailRepo : TableStorageBase<SentEmailDTO>, ISentEmailRepo
    {
        private readonly IAdminLogger _adminLogger;
        private readonly IUserAdminSettings _userAdminSettings;

        private readonly SentEmailXRefRepo _sentEmailXrefRepo;

        public SentEmailRepo(IUserAdminSettings settings, IAdminLogger logger) :
        base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
            _adminLogger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userAdminSettings = settings ?? throw new ArgumentNullException(nameof(settings));
            _sentEmailXrefRepo = new SentEmailXRefRepo(settings, logger);
        }

        public async Task AddSentEmailAsync(SentEmail sentEmail)
        {
            var dto = SentEmailDTO.FromSentEmail(sentEmail);
            await InsertAsync(dto);

            await _sentEmailXrefRepo.AddAsync(sentEmail.ExternalMessageId, sentEmail.Org.Id, dto.RowKey);
        }

        public async Task<SentEmail> GetSentEmailAsync(string externalMessageId)
        {
            var xref = await _sentEmailXrefRepo.ResolveAsync(externalMessageId);
            if(xref == null)
            {
                return null;
            }

            var record = await GetAsync(xref.OrgId, xref.SentEmailRowKey, false);
            if (record == null)
            {
                record = await GetAsync(externalMessageId, false);
                if (record == null)
                    return null;
            }

            return record.ToSentEmail();
        }

        public async Task<ListResponse<SentEmail>> GetSentEmailForOrgAsync(string orgId, ListRequest listRequest)
        {
            var records = await this.GetPagedResultsAsync(orgId, listRequest);
            return ListResponse<SentEmail>.Create(records.Model.Select(rec => rec.ToSentEmail()), listRequest);
        }

        public async Task<ListResponse<SentEmail>> GetSentEmailForMailerAsync(string orgId, string mailerId, ListRequest listRequest)
        {
            var records = await this.GetPagedResultsAsync(orgId,  listRequest, FilterOptions.Create(nameof(SentEmailDTO.MailerId), FilterOptions.Operators.Equals, mailerId));
            return ListResponse<SentEmail>.Create(records.Model.Select(rec => rec.ToSentEmail()), listRequest);
        }

        public async Task<ListResponse<SentEmail>> GetIndividualSentEmailForOrgAsync(string orgId, ListRequest listRequest)
        { 
            var records = await this.GetPagedResultsAsync(orgId, listRequest, FilterOptions.Create(nameof(SentEmailDTO.IndividualMessage), FilterOptions.Operators.Equals, true));
            return ListResponse<SentEmail>.Create(records.Model.Select(rec => rec.ToSentEmail()), listRequest);
        }


        public Task UpdateSentEmailAsync(SentEmail sentEmail)
        {
            return UpdateAsync(SentEmailDTO.FromSentEmail(sentEmail));
        }

        public async Task<SentEmail> GetSentEmailAsync(string orgId, string internalMessageId)
        {
            var sentEmail = await this.FindAsync(orgId, FilterOptions.Create(nameof(SentEmailDTO.InternalMessageId), FilterOptions.Operators.Equals, internalMessageId));
            if(sentEmail == null)
            {
                throw new RecordNotFoundException(nameof(SentEmail), $"OrgId={orgId}, internalMessgaeId={internalMessageId}");
            }

            return sentEmail.ToSentEmail();
        }

        private string GetContainerName(SentEmail sentEmail)
        {
            var dateStamp = sentEmail.SentDate.ToDateTime();
            var containerName =  $"emailarchive{sentEmail.OrgNameSpace}{dateStamp.Year:0000}{dateStamp.Month:00}".ToLower();
            return containerName;
        }

        private async Task<BlobContainerClient> GetBlobContainerClient(string containerName)
        {
            var baseuri = $"https://{_userAdminSettings.UserTableStorage.AccountId}.blob.core.windows.net";
            var uri = new Uri(baseuri);

            var connectionString = $"DefaultEndpointsProtocol=https;AccountName={_userAdminSettings.UserTableStorage.AccountId};AccountKey={_userAdminSettings.UserTableStorage.AccessKey}";
            var blobClient = new BlobServiceClient(connectionString);
            try
            {
                var blobContainerClient = blobClient.GetBlobContainerClient(containerName);
                return blobContainerClient;
            }
            catch (Exception)
            {
                var container = await blobClient.CreateBlobContainerAsync(containerName);

                return container.Value;
            }
        }

        private string GetBlobName(SentEmail sentEmail)
        {
            return $"{sentEmail.InternalMessageId}.eml";
        }


        public async Task AddEmailBodyAsync(SentEmail email, string body)
        {
            var retryCount = 0;
            Exception ex = null;
            while (retryCount++ < 5)
            {
                try
                {
                    var containerName = GetContainerName(email);
                    var containerClient = await GetBlobContainerClient(containerName);
                    await containerClient.CreateIfNotExistsAsync();
                    var blobName = GetBlobName(email);
                    var buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(body);
                    var blobClient = containerClient.GetBlobClient(blobName);
                    await blobClient.UploadAsync(new BinaryData(buffer));
                    return;
                }
                catch (Exception exc)
                {
                    ex = exc;
                    _adminLogger.AddException("AttachmentRepo_AddAttachmentAsync", ex);
                    Console.WriteLine("Exception deserializeing: " + ex.Message);
                    await Task.Delay(retryCount * 250);
                    if (retryCount == 4)
                        throw;
                }
            }
        }


        public async Task<string> GetEmailBodyAsync(SentEmail email)
        {
            var retryCount = 0;
            Exception ex = null;
            while (retryCount++ < 5)
            {
                try
                {
                    var containerName = GetContainerName(email);
                    var containerClient = await GetBlobContainerClient(containerName);
                    await containerClient.CreateIfNotExistsAsync();
                    var blobName = GetBlobName(email);
                    var blobClient = containerClient.GetBlobClient(blobName);
                    var content = await blobClient.DownloadContentAsync();
                    var text = System.Text.ASCIIEncoding.ASCII.GetString(content.Value.Content.ToArray());
                    return text;                    
                }
                catch (Exception exc)
                {
                    ex = exc;
                    _adminLogger.AddException("AttachmentRepo_GetAttachmentAsync", ex);
                    Console.WriteLine("Exception deserializeing: " + ex.Message);
                    await Task.Delay(retryCount * 250);
                    if (retryCount == 4)
                        throw;
                }
            }

            /* nop */
            return null;
        }
    } 
}
