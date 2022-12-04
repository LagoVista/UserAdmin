using LagoVista.Core.Models;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class UserAccessManager : IIUserAccessManager
    {
        private readonly IUserSecurityServices _userSecurityService;
        private readonly IModuleRepo _moduleRepo;
        
        public UserAccessManager(IUserSecurityServices userSecurityService, IModuleRepo moduleRepo)
        {
            _userSecurityService = userSecurityService ?? throw new ArgumentNullException(nameof(userSecurityService)); 
            _moduleRepo = moduleRepo ?? throw new ArgumentNullException(nameof(moduleRepo));
        }


        public async Task<List<ModuleSummary>> GetUserModulesAsync(string userId, string orgId)
        {
            var userAccessList = await _userSecurityService.GetRoleAccessForUserAsync(userId, orgId);
            var modules = await _moduleRepo.GetAllModulesAsync();
  
            var userRoles = await _userSecurityService.GetRolesForUserAsync(userId, orgId);
            if(userRoles.Any(rol=>rol.Key == DefaultRoleList.OWNER))
            {
                return modules;
            }
            
            var userModules = new List<ModuleSummary>();

            userModules.AddRange(modules.Where(mod => !mod.RestrictByDefault));

            foreach (var access in userAccessList)
            {
                if (access.Read == 1 && !userModules.Any(mod=>mod.Id == access.Module.Id))
                {
                    var module = modules.Single(mod => mod.Id == access.Module.Id);
                    userModules.Add(module);
                }
            }

            return userModules;
        }
        public async Task<Module> GetUserModuleAsync(string moduleKey, string userId, string orgId)
        {
            var module = await _moduleRepo.GetModuleByKeyAsync(moduleKey);
            
            var userAccessList = await _userSecurityService.GetModuleRoleAccessForUserAsync(module.Id, userId, orgId);
            var modules = await _moduleRepo.GetAllModulesAsync();
           
            foreach(var area in module.Areas)
            {
                if (!area.RestrictByDefault)
                {
                    area.UserAccess = UserAccess.GetFullAccess();

                    foreach (var page in area.Pages)
                    {
                        if (!page.RestrictByDefault)
                        {
                            page.UserAccess = UserAccess.GetFullAccess();
                            foreach(var feature in page.Features)
                            {
                                if (!feature.RestrictByDefault)
                                    feature.UserAccess = UserAccess.GetFullAccess();
                            }
                        }
                    }
                }
            }

            foreach (var access in userAccessList)
            {
                if (!EntityHeader.IsNullOrEmpty(access.Feature))
                {
                    var area = module.Areas.Single(ara => ara.Id == access.Area.Id);
                    var page = area.Pages.Single(pge => pge.Id == access.Page.Id);
                    var feature = page.Features.Single(ftr => ftr.Id == access.Feature.Id);
                    if (feature == null)
                    {
                        feature.UserAccess = new UserAccess()
                        {
                            Create = access.Create,
                            Read = access.Read,
                            Update = access.Update,
                            Delete = access.Delete
                        };
                    }
                    else
                    {
                        feature.UserAccess.Create = access.Create == -1 || feature.UserAccess.Create == -1 ? -1 : access.Create == 0 ? feature.UserAccess.Create : 1;
                        feature.UserAccess.Read = access.Read == -1 || feature.UserAccess.Read == -1 ? -1 : access.Read == 0 ? page.UserAccess.Read : 1;
                        feature.UserAccess.Update = access.Update == -1 || feature.UserAccess.Update == -1 ? -1 : access.Update == 0 ? feature.UserAccess.Update : 1;
                        feature.UserAccess.Delete = access.Delete == -1 || feature.UserAccess.Delete == -1 ? -1 : access.Delete == 0 ? feature.UserAccess.Delete : 1;
                    }
                }
                else if (!EntityHeader.IsNullOrEmpty(access.Page))
                {
                    var area = module.Areas.Single(ara => ara.Id == access.Area.Id);
                    var page = area.Pages.Single(pge => pge.Id == access.Page.Id);
                    if (page.UserAccess == null)
                    {
                        page.UserAccess = new UserAccess()
                        {
                            Create = access.Create,
                            Read = access.Read,
                            Update = access.Update,
                            Delete = access.Delete
                        };
                    }
                    else
                    {
                        page.UserAccess.Create = access.Create == -1 || page.UserAccess.Create == -1 ? -1 : access.Create == 0 ? page.UserAccess.Create : 1;
                        page.UserAccess.Read = access.Read == -1 || page.UserAccess.Read == -1 ? -1 : access.Read == 0 ? page.UserAccess.Read : 1;
                        page.UserAccess.Update = access.Update == -1 || page.UserAccess.Update == -1 ? -1 : access.Update == 0 ? page.UserAccess.Update : 1;
                        page.UserAccess.Delete = access.Delete == -1 || page.UserAccess.Delete == -1 ? -1 : access.Delete == 0 ? page.UserAccess.Delete : 1;
                    }
                }
                else if (!EntityHeader.IsNullOrEmpty(access.Area))
                {
                    var area = module.Areas.Single(ara => ara.Id == access.Area.Id);
                    if (area.UserAccess == null)
                    {
                        area.UserAccess = new UserAccess()
                        {
                            Create = access.Create,
                            Read = access.Read,
                            Update = access.Update,
                            Delete = access.Delete
                        };
                    }
                    else
                    {
                        area.UserAccess.Create = access.Create == -1 || area.UserAccess.Create == -1 ? -1 : access.Create == 0 ? area.UserAccess.Create : 1;
                        area.UserAccess.Read = access.Read == -1 || area.UserAccess.Read == -1 ? -1 : access.Read == 0 ? area.UserAccess.Read : 1;
                        area.UserAccess.Update = access.Update == -1 || area.UserAccess.Update == -1 ? -1 : access.Update == 0 ? area.UserAccess.Update : 1;
                        area.UserAccess.Delete = access.Delete == -1 || area.UserAccess.Delete == -1 ? -1 : access.Delete == 0 ? area.UserAccess.Delete : 1;
                    }
                }
                else
                {
                    if (module.UserAccess == null)
                    {
                        module.UserAccess = new UserAccess()
                        {
                            Create = access.Create,
                            Read = access.Read,
                            Update = access.Update,
                            Delete = access.Delete
                        };
                    }
                    else
                    {
                        module.UserAccess.Create = access.Create == -1 || module.UserAccess.Create == -1 ? -1 : access.Create == 0 ? module.UserAccess.Create : 1;
                        module.UserAccess.Read = access.Read == -1 || module.UserAccess.Read == -1 ? -1 : access.Read == 0 ? module.UserAccess.Read : 1;
                        module.UserAccess.Update = access.Update == -1 || module.UserAccess.Update == -1 ? -1 : access.Update == 0 ? module.UserAccess.Update : 1;
                        module.UserAccess.Delete = access.Delete == -1 || module.UserAccess.Delete == -1 ? -1 : access.Delete == 0 ? module.UserAccess.Delete : 1;
                    }
                }
            }

            return module;
        }
    }
}
