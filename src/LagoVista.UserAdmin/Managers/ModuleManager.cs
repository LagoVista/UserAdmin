// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e6809b6dceae8aef3932ec2f13fb3f88ec424aea22600235d7cb5067595bf0fb
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
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
        private IUserManager _userManager;
        private IOrganizationRepo _orgRepo;

        public ModuleManager(IModuleRepo moduleRepo, IUserManager userManager, IOrganizationRepo orgRepo, ILogger logger, IAppConfig appConfig, IDependencyManager dependencyManager, ISecurity security) : base(logger, appConfig, dependencyManager, security)
        {
            _moduleRepo = moduleRepo ?? throw new ArgumentNullException(nameof(moduleRepo));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _orgRepo = orgRepo ?? throw new ArgumentNullException(nameof(orgRepo));
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

        public async Task<ListResponse<ModuleSummary>> GetAllModulesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(Module));
            return await _moduleRepo.GetAllModulesForOrgAsync(org.Id, listRequest);            
        }

        public async Task<Module> GetModuleAsync(string id, EntityHeader org, EntityHeader user)
        {
            var module = await _moduleRepo.GetModuleAsync(id);
            await AuthorizeAsync(module, AuthorizeActions.Read, user, org);
            return module;
        }

        public async Task<ListResponse<ModuleSummary>> SysAdminGetModuleAsync(string orgId, EntityHeader user)
        {
            var editingUser = await _userManager.FindByIdAsync(user.Id);
            var org = await _orgRepo.GetOrganizationAsync(orgId);
            if (!editingUser.IsSystemAdmin)
            {
                return ListResponse<ModuleSummary>.FromErrors(new ErrorMessage() { Message = "Must be a System Admin to load all module by org" });
            }
            return ListResponse<ModuleSummary>.Create( await _moduleRepo.GetModulesForOrgAndPublicAsync(orgId, org.IsForProductLine));
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
                new UiCategory() { Id = "15C14BE40FDA4E9587EFA66502F05F14", Key="iot", Name = UserAdminResources.ModuleCategory_IoT, Summary = UserAdminResources.ModuleCategory_IoT_Summary},
                new UiCategory() { Id = "15C14BE40FDA4E9587EFA66502F05F15", Key="admin", Name = UserAdminResources.ModuleCategory_Admin, Summary = UserAdminResources.ModuleCategory_Admin_Summary},
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

        public async Task<ListResponse<ModuleSummary>> GetAllModulesAsync(EntityHeader user, ListRequest listRequest)
        {
            var editingUser = await _userManager.FindByIdAsync(user.Id);
            if (!editingUser.IsSystemAdmin)
            {
                return ListResponse<ModuleSummary>.FromErrors(new ErrorMessage() { Message = "Must be a System Admin to load all modules" });
            }

            return await _moduleRepo.GetAllModulesAsync(listRequest);
        }
    }
}
