using LagoVista.CloudStorage;
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Documents.Client;
using System;
using System.Linq;
using System.Threading.Tasks;
using LagoVista.Core;
using System.Collections.Generic;
using LagoVista.Core.Validation;

namespace LagoVista.UserAdmin.Repos.Repos.Orgs
{
    public class SubscriptionResourceRepo : DocumentDBRepoBase<SubscriptionResource>, ISubscriptionResourceRepo
    {
        bool _shouldConsolidateCollections;
        private readonly IAdminLogger _logger;

        public SubscriptionResourceRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger, ICacheProvider cacheProvider = null) :
                base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger, cacheProvider)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        protected override bool ShouldConsolidateCollections
        {
            get { return _shouldConsolidateCollections; }
        }

        public async Task<ListResponse<SubscriptionResource>> GetResourcesForSubscriptionAsync(Guid subscriptionId, ListRequest listRequest, string orgId)
        {
            try
            {
                var options = new FeedOptions()
                {
                    MaxItemCount = (listRequest.PageSize == 0) ? 50 : listRequest.PageSize
                };

                if (!String.IsNullOrEmpty(listRequest.NextRowKey))
                {
                    options.RequestContinuation = listRequest.NextRowKey;
                }

                var documentLink = await GetCollectionDocumentsLinkAsync();

                var docQuery = Client.CreateDocumentQuery<SubscriptionResource>(documentLink, options)
                    .Where(res=>res.Subscription.Id == subscriptionId.ToString()).AsDocumentQuery();

                var result = await docQuery.ExecuteNextAsync<SubscriptionResource>();
                if (result == null)
                {
                    throw new Exception("Null Response from Query");
                }

                var listResponse = ListResponse<SubscriptionResource>.Create(result);
                listResponse.NextRowKey = result.ResponseContinuation;
                listResponse.PageSize = result.Count;
                listResponse.HasMoreRecords = result.Count == listRequest.PageSize;
                listResponse.PageIndex = listRequest.PageIndex;

                return listResponse;
            }
            catch (Exception ex)
            {
                _logger.AddException("DocumentDBBase", ex, typeof(SubscriptionResource).Name.ToKVP("entityType"));

                var listResponse = ListResponse<SubscriptionResource>.Create(new List<SubscriptionResource>());
                listResponse.Errors.Add(new ErrorMessage(ex.Message));
                return listResponse;
            }
        }
    }
}
