using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class FunctionMapManager : ManagerBase, IFunctionMapManager
    {
        private readonly IFunctionMapRepo _functionMapRepo;

        public FunctionMapManager(IFunctionMapRepo functionMapRepo,  IDependencyManager depManager, ISecurity security, IAdminLogger logger, IAppConfig appConfig) : base(logger, appConfig, depManager, security)
        {
            _functionMapRepo = functionMapRepo ?? throw new ArgumentNullException(nameof(functionMapRepo));
        }

        public async Task<InvokeResult> AddFunctionMapAsync(FunctionMap map, EntityHeader org, EntityHeader user)
        {
            await _functionMapRepo.AddFunctionMapAsync(map);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> DeleteFunctionMapAsync(string id, EntityHeader org, EntityHeader user)
        {
            await _functionMapRepo.DeleteFunctionMapAsync(id);
            return InvokeResult.Success;
        }

        public Task<FunctionMap> GetFunctionMapAsync(string id, EntityHeader org, EntityHeader user)
        {
            return _functionMapRepo.GetFunctionMapAsync(id);
        }

        public Task<FunctionMap> GetFunctionMapByKeyAsync(string key, EntityHeader org, EntityHeader user)
        {
            return _functionMapRepo.GetFunctionMapByKeyAsync(org.Id, key);
        }

        public Task<FunctionMap> GetTopLevelFunctionMapAsync(EntityHeader org, EntityHeader user)
        {
            return _functionMapRepo.GetTopLevelFunctionMapAsync(org.Id);
        }

        public async Task<InvokeResult> UpdateFunctionMapAsync(FunctionMap map, EntityHeader org, EntityHeader user)
        {
            await _functionMapRepo.UpdateFunctionMapAsync(map);
            return InvokeResult.Success;
        }
    }
}
