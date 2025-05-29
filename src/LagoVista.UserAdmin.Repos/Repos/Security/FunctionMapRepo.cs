using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Interfaces;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Security
{
    public class FunctionMapRepo : DocumentDBRepoBase<FunctionMap>, IFunctionMapRepo
    {


        private ICacheProvider _cacheProvider;
        private IAdminLogger _adminLogger;
        public FunctionMapRepo(IUserAdminSettings settings, IDefaultRoleList defaultRoleList, IAdminLogger logger, ICacheProvider cacheProvider) :
           base(settings.UserStorage.Uri, settings.UserStorage.AccessKey, settings.UserStorage.ResourceName, logger, cacheProvider)
        {
            _adminLogger = logger ?? throw new ArgumentNullException(nameof(logger));   
            _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
        }



        protected override bool ShouldConsolidateCollections
        {
            get => true;
        }

        public Task AddFunctionMapAsync(FunctionMap map)
        {
            return CreateDocumentAsync(map);   
        }

        public Task DeleteFunctionMapAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<FunctionMap> GetFunctionMapAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public async Task<FunctionMap> GetFunctionMapByKeyAsync(string orgId, string key)
        {
            var topLevel = await QueryAsync(mp => mp.Key == key && mp.OwnerOrganization.Id == orgId);
            if (topLevel.Count() == 1)
                return topLevel.Single();

            topLevel = await QueryAsync(mp => mp.Key == key && mp.IsPublic);
            if (topLevel.Count() > 1)
            {
                throw new Exception($"Found more than one function map by key {key}.");
            }
            else
                return topLevel.SingleOrDefault();
        }

        public async Task<FunctionMap> GetTopLevelFunctionMapAsync(string orgId)
        {
            var topLevel = await QueryAsync(mp => mp.TopLevel && mp.OwnerOrganization.Id == orgId);
            if (topLevel.Count() == 1)
                return topLevel.Single();

            topLevel = await QueryAsync(mp => mp.TopLevel && mp.IsPublic);
            if (topLevel.Count() > 1)
            {
                throw new Exception("Should be only one public top level function map.");
            }
            else 
                return topLevel.SingleOrDefault();
        }

        public Task UpdateFunctionMapAsync(FunctionMap map)
        {
            return UpsertDocumentAsync(map);
        }
    }
}
