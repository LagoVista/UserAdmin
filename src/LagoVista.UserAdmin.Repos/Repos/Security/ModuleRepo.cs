using LagoVista.CloudStorage;
using LagoVista.CloudStorage.DocumentDB;
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

        public ModuleRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger, ICacheProvider cacheProvider) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger, cacheProvider)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
            this._cacheProvider = cacheProvider!;
        }

        protected override bool ShouldConsolidateCollections
        {
            get { return _shouldConsolidateCollections; }
        }

        public Task AddModuleAsync(Module module)
        {
            return this.CreateDocumentAsync(module);
        }

        public async Task DeleteModuleAsync(string id)
        {
            await _cacheProvider.RemoveAsync(ALL_MODULES_CACHE_KEY);
            var module = await GetModuleAsync(id);
            if (module == null)
                throw new ArgumentNullException(nameof(Module), id);

            await _cacheProvider.RemoveAsync($"{MODULE_CACHE_KEY}_{module.Key}");
            await this.DeleteDocumentAsync(id);
        }

        public async Task<List<ModuleSummary>> GetAllModulesAsync()
        {
            var allModulesJson = await _cacheProvider.GetAsync(ALL_MODULES_CACHE_KEY);
            if (String.IsNullOrEmpty(allModulesJson))
            {
                var lists = await QueryAsync(rec => true);
                var summaries = lists.Select(mod => mod.CreateSummary()).ToList();
                await _cacheProvider.AddAsync(ALL_MODULES_CACHE_KEY, JsonConvert.SerializeObject(summaries));
                return summaries;
            }
            else
            {
                return JsonConvert.DeserializeObject<List<ModuleSummary>>(allModulesJson);
            }

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

                await _cacheProvider.AddAsync($"{MODULE_CACHE_KEY}_{key}", JsonConvert.SerializeObject(module));

                return module;
            }
            else
            {
                return JsonConvert.DeserializeObject<Module>(json);
            }
        }

        public async Task<Module> GetModuleByKeyAsync(string key, string orgId)
        {
            return (await QueryAsync(mod => mod.Key == key && mod.OwnerOrganization.Id == orgId)).FirstOrDefault();
        }

        public async Task UpdateModuleAsync(Module module)
        {
            await _cacheProvider.RemoveAsync(ALL_MODULES_CACHE_KEY);
            await _cacheProvider.RemoveAsync($"{MODULE_CACHE_KEY}_{module.Key}");
            await this.UpsertDocumentAsync(module);
        }
    }
}
