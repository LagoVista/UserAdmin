// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e3311c84e5feeb14e1040b097e8920631d9a90e6eb7a16a890723cf2eaf9c566
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Models.ML;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Account;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Diagnostics;
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

        public async Task<InvokeResult<MostRecentlyUsed>> AddMostRecentlyUsedAsync(MostRecentlyUsedItem mostRecentlyUsedItem, EntityHeader org, EntityHeader user)
        {

            mostRecentlyUsedItem.DateAdded = DateTime.UtcNow.ToJSONString();
            mostRecentlyUsedItem.LastAccessed = DateTime.UtcNow.ToJSONString();

            var mru = await GetMostRecentlyUsedAsync(org, user);
            var sw = Stopwatch.StartNew();

            if (String.IsNullOrEmpty(mru.Result.Key))
                mru.Result.Key = Guid.NewGuid().ToId().ToLower();

            var existing = mru.Result.All.SingleOrDefault(itm => itm.Link == mostRecentlyUsedItem.Link);
            if (existing != null)
                mru.Result.All.Remove(existing);

            mru.Result.All.Insert(0, mostRecentlyUsedItem);
            if (mru.Result.All.Count > 50)
            {
                mru.Result.All.RemoveAt(50);
            }

            if (!String.IsNullOrEmpty(mostRecentlyUsedItem.ModuleKey))
            {
                var module = mru.Result.Modules.SingleOrDefault(mod => mod.ModuleKey == mostRecentlyUsedItem.ModuleKey);
                if (module == null)
                {
                    module = new MostRecentlyUsedModule() { ModuleKey = mostRecentlyUsedItem.ModuleKey };
                    mru.Result.Modules.Add(module);
                }
                else
                {
                    var existingModMRU = module.Items.SingleOrDefault(itm => itm.Link == mostRecentlyUsedItem.Link);
                    if (existingModMRU != null)
                        module.Items.Remove(existingModMRU);
                }

                module.Items.Insert(0, mostRecentlyUsedItem);
                if (module.Items.Count > 50)
                {
                    module.Items.RemoveAt(50);
                }
            }

            mru.Timings.Add(new ResultTiming() { Key = "Processing", Ms = sw.Elapsed.TotalMilliseconds });
            sw.Restart();

            await _mruRepo.UpdateMostRecentlyUsedAsync(mru.Result);
            mru.Timings.Add(new ResultTiming() { Key = "Updated MRU in storage.", Ms = sw.Elapsed.TotalMilliseconds });

            return  mru;
        }

        public async Task<InvokeResult> ClearMostRecentlyUsedAsync(EntityHeader org, EntityHeader user)
        {
            await _mruRepo.DeleteMostRecentlyUsedAsync(org.Id, user.Id);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult<MostRecentlyUsed>> GetMostRecentlyUsedAsync(EntityHeader org, EntityHeader user)
        {
            var result = new InvokeResult<MostRecentlyUsed>();

            var sw = Stopwatch.StartNew();
            var mru = await _mruRepo.GetMostRecentlyUsedAsync(org.Id, user.Id);

            if (mru == null)
            {
                result.Timings.Add(new ResultTiming() { Key = "Dit not get MRU from storage", Ms = sw.Elapsed.TotalMilliseconds });
                sw.Restart();
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
                result.Timings.Add(new ResultTiming() { Key = "MRU does not exist, created and inserted", Ms = sw.Elapsed.TotalMilliseconds });
            }
            else
                result.Timings.Add(new ResultTiming() { Key = "Got MRU from storage", Ms = sw.Elapsed.TotalMilliseconds });

            result.Result = mru;

            return result;
        }
    }
}
