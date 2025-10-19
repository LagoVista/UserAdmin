// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 633e85d7d59a5239bb33a4899d921d2d75c2953db6a1bc5bdb7037f3e41b6e19
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Interfaces;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using Newtonsoft.Json;
using System;
using System.Linq;
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

        private string GetCacheKey(FunctionMap map)
        {
            if(map.IsPublic)
            {
                if(map.TopLevel)
                {
                    return $"funcitonmap-public-root";
                }
                else
                    return $"functionmap-public-{map.Key}";
            }
            else
            {
                if(map.TopLevel)
                {
                    return $"functionmap-{map.OwnerOrganization.Id}-root";
                }
                else
                    return $"functionmap-{map.OwnerOrganization.Id}-{map.Key}";
            }
        }

        private string GetCacheKey(string key)
        {
            return $"functionmap-{key}";
        }

        private string GetCacheKey(string orgid, string key)
        {
            return $"functionmap-{orgid}-{key}";

        }

        public async Task AddFunctionMapAsync(FunctionMap map)
        {
            if (map.TopLevel && map.IsPublic)
            {
                var existing = await QueryAsync(qry => qry.IsPublic && qry.TopLevel);
                if (existing.Any())
                {
                    throw new InvalidOperationException($"An existing top level public function map exists.  Existing: {existing.First().Name}/{existing.First().Id}.  Keys must be unique within oganization.");
                }
            }
            else if (map.TopLevel)
            {
                var existing = await QueryAsync(qry => qry.IsPublic && qry.OwnerOrganization.Id == map.OwnerOrganization.Id);
                if (existing.Any())
                {
                    throw new InvalidOperationException($"An existing top level for the {map.OwnerOrganization.Text} org, function map exists.  Existing: {existing.First().Name}/{existing.First().Id}.  Keys must be unique within oganization.");
                }
            }
            else if (map.IsPublic)
            {
                var existing = await QueryAsync(qry => qry.Key == map.Key && qry.IsPublic);
                if (existing.Any())
                {
                    throw new InvalidOperationException($"An existing public function map with the key [{map.Key}] aleady exists. Existing: {existing.First().Name}/{existing.First().Id}.  Keys must be unique within oganization.");
                }
            }
            else
            {
                var existing = await QueryAsync(qry => qry.Key == map.Key && qry.OwnerOrganization.Id == map.OwnerOrganization.Id);
                if (existing.Any())
                {
                    throw new InvalidOperationException($"An existing module with that key [{map.Key}] already exists for the {map.OwnerOrganization.Text} org, Existing: {existing.First().Name}/{existing.First().Id}.  Keys must be unique within oganization.");
                }
            }
           
            await CreateDocumentAsync(map);
            var json = JsonConvert.SerializeObject(map);
            var key = GetCacheKey(map);
            await _cacheProvider.AddAsync(key, json);          
        }

        public async Task DeleteFunctionMapAsync(string id)
        {
            await DeleteDocumentAsync(id);
        }

        public Task<FunctionMap> GetFunctionMapAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public async Task<FunctionMap> GetFunctionMapByKeyAsync(string orgId, string key)
        {
            var cacheKey = GetCacheKey(orgId, key);
            var json = await _cacheProvider.GetAsync(cacheKey);
            if(!String.IsNullOrEmpty(json))
                return JsonConvert.DeserializeObject<FunctionMap>(json);

            cacheKey = GetCacheKey(key);
            json = await _cacheProvider.GetAsync(cacheKey);
            if (!String.IsNullOrEmpty(json))
                return JsonConvert.DeserializeObject<FunctionMap>(json);

            var maps = await QueryAsync(mp => mp.Key == key && mp.OwnerOrganization.Id == orgId);
            if (maps.Count() == 1)
            {
                var map = maps.Single();
                cacheKey = GetCacheKey(orgId, key);
                await _cacheProvider.AddAsync(cacheKey, JsonConvert.SerializeObject(map));
                return map;
            }

            maps = await QueryAsync(mp => mp.Key == key && mp.IsPublic);
            if (maps.Count() > 1)
            {
                throw new Exception($"Found more than one function map by key {key}.");
            }
            else
            {
                if(maps.Count() == 0)
                {
                    return null;
                }

                var map = maps.Single();
                
                cacheKey = GetCacheKey(key);
                json = JsonConvert.SerializeObject(map);
                await _cacheProvider.AddAsync(cacheKey, json);

                return map;
            }
        }

        public async Task<FunctionMap> GetTopLevelFunctionMapAsync(string orgId)
        {
            var cacheKey = $"functionmap-{orgId}-root";
            var json = await _cacheProvider.GetAsync(cacheKey);
            if (!String.IsNullOrEmpty(json))
                return JsonConvert.DeserializeObject<FunctionMap>(json);

            cacheKey = $"funcitonmap-public-root";

            json = await _cacheProvider.GetAsync(cacheKey);
            if (!String.IsNullOrEmpty(json))
                return JsonConvert.DeserializeObject<FunctionMap>(json);

            var maps = await QueryAsync(mp => mp.TopLevel && mp.OwnerOrganization.Id == orgId);
            if (maps.Count() == 1)
            {
                var map = maps.Single();
                cacheKey = $"functionmap-{orgId}-root";
                await _cacheProvider.AddAsync(cacheKey, JsonConvert.SerializeObject(map));
                return map;
            }

            maps = await QueryAsync(mp => mp.TopLevel && mp.IsPublic);
            if (maps.Count() > 1)
            {
                throw new Exception("Should be only one public top level function map.");
            }
            else
            {
                if (maps.Count() == 0)
                {
                    return null;
                }

                var map = maps.Single();
                cacheKey = $"funcitonmap-public-root";
                json = JsonConvert.SerializeObject(map);
                await _cacheProvider.AddAsync(cacheKey, json);
                return map;
            }
        }

        public async Task UpdateFunctionMapAsync(FunctionMap map)
        {
            var cacheKey = GetCacheKey(map);
            await UpsertDocumentAsync(map);
            await _cacheProvider.AddAsync(cacheKey, JsonConvert.SerializeObject(map));
        }
    }
}
