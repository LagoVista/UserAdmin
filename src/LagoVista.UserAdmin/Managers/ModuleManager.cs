using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static LagoVista.Core.Models.AuthorizeResult;

namespace LagoVista.UserAdmin.Managers
{
    public class ModuleManager : ManagerBase, IModuleManager
    {
        private IModuleRepo _moduleRepo;

        public ModuleManager(IModuleRepo moduleRepo, ILogger logger, IAppConfig appConfig, IDependencyManager dependencyManager, ISecurity security) : base(logger, appConfig, dependencyManager, security)
        {
            _moduleRepo = moduleRepo ?? throw new ArgumentNullException(nameof(moduleRepo));
        }

        public async Task<InvokeResult> AddModuleAsync(Module module, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(module, AuthorizeActions.Create, user, org);
            ValidationCheck(module, Actions.Create);
            await _moduleRepo.AddModuleAsync(module);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> DeleteModuleAsync(string id, EntityHeader org, EntityHeader user)
        {
            var module = await _moduleRepo.GetModuleAsync(id);
            await AuthorizeAsync(module, AuthorizeActions.Delete, user, org);
            await ConfirmNoDepenenciesAsync(module);
            await _moduleRepo.DeleteModuleAsync(id);
            return InvokeResult.Success;
        }

        public async Task<List<ModuleSummary>> GetAllModulesAsync(EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(Module));
            return await _moduleRepo.GetAllModulesAsync();            
        }

        public async Task<Module> GetModuleAsync(string id, EntityHeader org, EntityHeader user)
        {
            var module = await _moduleRepo.GetModuleAsync(id);
            await AuthorizeAsync(module, AuthorizeActions.Read, user, org);
            return module;
        }

        public async Task<Module> GetModuleByKeyAsync(string key, EntityHeader org, EntityHeader user)
        {
            var module = await _moduleRepo.GetModuleByKeyAsync(key);
            await AuthorizeAsync(module, AuthorizeActions.Read, user, org);
            return module;
        }

        public List<UiCategory> GetTopLevelCategories()
        {
            return new List<UiCategory>()
            {
                new UiCategory() { Id = "15C14BE40FDA4E9587EFA66502F05F16", Key="tools", Name = UserAdminResources.ModuleCategory_Tools, Summary = UserAdminResources.ModuleCategory_Tools_Summary},
                new UiCategory() { Id = "26C14BE40FDA4E9587EFA66502F05F44", Key="support", Name = UserAdminResources.ModuleCategory_Support, Summary = UserAdminResources.ModuleCategory_Support_Summary},
                new UiCategory() { Id = "37C14BE40FDA4E9587EFA66502F05F23", Key="enduser", Name = UserAdminResources.ModuleCateogry_EndUser, Summary = UserAdminResources.ModuleCateogry_EndUser_Summary},
                new UiCategory() { Id = "48C14BE40FDA4E9587EFA66502F05F82", Icon="", Key="other", Name = UserAdminResources.ModuleCateogry_Other},

            };
        }

        public async Task<InvokeResult> UpdateModuleAsync(Module module, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(module, AuthorizeActions.Update, user, org);
            ValidationCheck(module, Actions.Create);
            await _moduleRepo.UpdateModuleAsync(module);

            return InvokeResult.Success;
        }
    }
}
