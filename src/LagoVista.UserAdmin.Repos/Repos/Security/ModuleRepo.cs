using LagoVista.CloudStorage;
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Security
{
    public class ModuleRepo : DocumentDBRepoBase<Module>, IModuleRepo
    {
        private readonly bool _shouldConsolidateCollections;

        public ModuleRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections
        {
            get { return _shouldConsolidateCollections; }
        }

        public Task AddModuleAsync(Module module)
        {
            return this.CreateDocumentAsync(module);
        }

        public Task DeleteModuleAsync(string id)
        {
            return this.DeleteDocumentAsync(id);
        }

        public async Task<List<ModuleSummary>> GetAllModulesAsync()
        {
            var lists = await QueryAsync(rec => true);
            return lists.Select(mod => mod.CreateSummary()).ToList();
        }

        public Task<Module> GetModuleAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task UpdateModuleAsync(Module module)
        {
            return this.UpsertDocumentAsync(module);
        }
    }
}
