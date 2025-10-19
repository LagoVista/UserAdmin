// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 5d2a0ba94ad46233223b748d806a2e56c01d74ec19e6f5e91c164ed19f4da03e
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.CloudStorage;
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Security
{
    public class ModuleRepo : DocumentDBRepoBase<Module>, IModuleRepo
    {
        public const string ALL_MODULES_CACHE_KEY = "NUVIOT_ALL_MODULES";
        public const string MODULE_CACHE_KEY = "NUVIOT_MODULE_";


        private readonly bool _shouldConsolidateCollections;

        private ICacheProvider _cacheProvider;
        private IAdminLogger _adminLogger;

        public ModuleRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger, ICacheProvider cacheProvider) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger, cacheProvider)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
            this._cacheProvider = cacheProvider!;
            this._adminLogger = logger;
        }

        protected override bool ShouldConsolidateCollections
        {
            get { return _shouldConsolidateCollections; }
        }

        public async Task AddModuleAsync(Module module)
        {
            await _cacheProvider.RemoveAsync(ALL_MODULES_CACHE_KEY);
            await this.CreateDocumentAsync(module);
        }

        public async Task DeleteModuleAsync(string id)
        {
            var module = await GetModuleAsync(id);
            if (module == null)
                throw new ArgumentNullException(nameof(Module), id);

            await this.DeleteDocumentAsync(id);
      
            await _cacheProvider.RemoveAsync($"{MODULE_CACHE_KEY}_{module.Key}");
            await _cacheProvider.RemoveAsync(ALL_MODULES_CACHE_KEY);
        }

        private async Task<List<ModuleSummary>> GetAll()
        {
            var allModulesJson = await _cacheProvider.GetAsync(ALL_MODULES_CACHE_KEY);
            if (String.IsNullOrEmpty(allModulesJson))
            {
                _adminLogger.Trace($"[ModuleRepo__GetAllModules] - CACHE-MISS - {ALL_MODULES_CACHE_KEY}");
                var lists = await QueryAsync(rec => !rec.IsDeleted.HasValue || rec.IsDeleted == false);
                var summaries = lists.OrderBy(mod => mod.SortOrder).Select(mod => mod.CreateSummary()).ToList();
                await _cacheProvider.AddAsync(ALL_MODULES_CACHE_KEY, JsonConvert.SerializeObject(summaries));
                return summaries;
            }
            else
            {
                var allModules = JsonConvert.DeserializeObject<List<ModuleSummary>>(allModulesJson);
                _adminLogger.Trace($"[ModuleRepo__GetAllModules] - CACHE-HIT - {ALL_MODULES_CACHE_KEY} - has {allModules.Count} modules");
                foreach(var module in allModules)
                {
                    if(String.IsNullOrWhiteSpace(module.RootPath))
                        module.RootPath = module.Key;
                }
                
                return allModules;
            }
        }

        public async Task<ListResponse<ModuleSummary>> GetAllModulesForOrgAsync(string orgId, ListRequest listRequest)
        {
            var all = await GetAll();
            return ListResponse<ModuleSummary>.Create(listRequest, 
                all.Where(org => org.OwnerOrgId == orgId && (org.IsDeleted == false || !org.IsDeleted.HasValue))
                .OrderBy( mod=>mod.UiCategory?.Text).ThenBy(mod=>mod.SortOrder)
                .Skip(listRequest.PageSize * (listRequest.PageIndex - 1)).Take(listRequest.PageSize));
        }

        public async Task<List<ModuleSummary>> GetModulesForOrgAndPublicAsync(string orgId, bool isForProductLine)
        {
            var modules = await GetAll();
            return modules.Where(mod => (!mod.IsDeleted.HasValue || mod.IsDeleted == false) && (mod.IsPublic || mod.OwnerOrgId == orgId || (mod.IsForProductLine && isForProductLine))).OrderBy(mod=>mod.UiCategory?.Text).ThenBy(mod=>mod.SortOrder).ToList();
        }

        public Task<Module> GetModuleAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public async Task<Module> GetModuleByKeyAsync(string key)
        {
            var json = await _cacheProvider.GetAsync($"{MODULE_CACHE_KEY}_{key}");
            if(string.IsNullOrEmpty(json))
            {
                var module = (await QueryAsync(mod => mod.Key == key)).FirstOrDefault();
                if (module == null)
                    return null;

                if (String.IsNullOrEmpty(module.RootPath))
                    module.RootPath = module.Key;

                await _cacheProvider.AddAsync($"{MODULE_CACHE_KEY}_{key}", JsonConvert.SerializeObject(module));

                return module;
            }
            else
            {
                var module =  JsonConvert.DeserializeObject<Module>(json);
                if (String.IsNullOrEmpty(module.RootPath))
                    module.RootPath = module.Key;

                return module;
            }
        }

        public async Task<Module> GetModuleByKeyAsync(string key, string orgId)
        {
            var module = (await QueryAsync(mod => mod.Key == key && mod.OwnerOrganization.Id == orgId)).FirstOrDefault();
            if (String.IsNullOrEmpty(module?.RootPath))
                module.RootPath = module.Key;

            return module;
        }


        public async Task UpdateModuleAsync(Module module)
        {
            await _cacheProvider.RemoveAsync(ALL_MODULES_CACHE_KEY);
            await _cacheProvider.RemoveAsync($"{MODULE_CACHE_KEY}_{module.Key}");
            await this.UpsertDocumentAsync(module);
        }

        public async Task<ListResponse<ModuleSummary>> GetAllModulesAsync(ListRequest listRequest)
        {
            return ListResponse<ModuleSummary>.Create((await GetAll()).OrderBy(mod=>mod.Name));
        }
    }
}
