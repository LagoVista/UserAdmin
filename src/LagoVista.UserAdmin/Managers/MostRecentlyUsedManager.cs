using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Account;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class MostRecentlyUsedManager : ManagerBase, IMostRecentlyUsedManager
    {
        private readonly IMostRecentlyUsedRepo _mruRepo;
        public MostRecentlyUsedManager(IMostRecentlyUsedRepo mruRepo, ILogger logger, IAppConfig appConfig, IDependencyManager dependencyManager, ISecurity security) : 
            base(logger, appConfig, dependencyManager, security)
        {
            _mruRepo = mruRepo ?? throw new ArgumentNullException(nameof(mruRepo));
        }

        public async Task<MostRecentlyUsed> AddMostRecentlyUsedAsync(MostRecentlyUsedItem mostRecentlyUsedItem, EntityHeader org, EntityHeader user)
        {
            var mru = await GetMostRecentlyUsedAsync(org, user);

            var existing = mru.All.SingleOrDefault(itm => itm.Link == mostRecentlyUsedItem.Link);
            if (existing != null)
                mru.All.Remove(existing);

            mru.All.Insert(0, mostRecentlyUsedItem);
            if(mru.All.Count > 8)
            {
                mru.All.RemoveAt(8);
            }

            if (!String.IsNullOrEmpty(mostRecentlyUsedItem.ModuleKey))
            {
                var module = mru.Modules.SingleOrDefault(mod => mod.ModuleKey == mostRecentlyUsedItem.ModuleKey);
                if (module == null)
                {
                    module = new MostRecentlyUsedModule() { ModuleKey = mostRecentlyUsedItem.ModuleKey };
                    mru.Modules.Add(module);
                }
                else
                {
                    var existingModMRU = module.Items.SingleOrDefault(itm => itm.Link == mostRecentlyUsedItem.Link);
                    if (existingModMRU != null)
                        module.Items.Remove(existingModMRU);
                }

                module.Items.Insert(0, mostRecentlyUsedItem);
                if (module.Items.Count > 8)
                {
                    module.Items.RemoveAt(8);
                }
            }

            await _mruRepo.UpdateMostRecentlyUsedAsync(mru);

            return mru;
        }

        public async Task<InvokeResult> ClearMostRecentlyUsedAsync(EntityHeader org, EntityHeader user)
        {
            await _mruRepo.DeleteMostRecentlyUsedAsync(org.Id, user.Id);
            return InvokeResult.Success;
        }

        public async Task<MostRecentlyUsed> GetMostRecentlyUsedAsync(EntityHeader org, EntityHeader user)
        {
            var mru = await _mruRepo.GetMostRecentlyUsedAsync(org.Id, user.Id);
            if (mru == null)
            {
                var timeStamp = DateTime.UtcNow.ToJSONString();
                mru = new MostRecentlyUsed()
                {
                    OwnerOrganization = org,
                    OwnerUser = user,
                    
                    IsPublic = false,

                    CreatedBy = user,
                    CreationDate = timeStamp,
                    LastUpdatedDate = timeStamp,
                    LastUpdatedBy = user,
                    Name = $"{user.Text}/{org.Text} - Most Recently Used Items"
                };
                await _mruRepo.AddMostRecentlyUsedAsync(mru);
            }

            return mru;
        }
    }
}
