using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Interfaces;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
        }

        public Task<FunctionMap> GetFunctionMapAsync(string id)
        {
        
            return GetDocumentAsync(id);
        }

        public Task<FunctionMap> GetFunctionMapByKeyAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task<FunctionMap> GetTopLevelFunctionMapAsync(string orgId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateFunctionMapAsync(FunctionMap map)
        {
            return UpsertDocumentAsync(map);
        }
    }
}
