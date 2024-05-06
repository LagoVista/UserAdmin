//#define VERBOSE

using LagoVista.Core.Models;
using LagoVista.IoT.Logging.Loggers;
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
        private readonly IAdminLogger _adminLogger;

        public UserAccessManager(IUserSecurityServices userSecurityService, IModuleRepo moduleRepo, IAdminLogger adminLogger)
        {
            _userSecurityService = userSecurityService ?? throw new ArgumentNullException(nameof(userSecurityService));
            _moduleRepo = moduleRepo ?? throw new ArgumentNullException(nameof(moduleRepo));
            _adminLogger = adminLogger ?? throw new ArgumentNullException(nameof(adminLogger));
        }

        public async Task<List<ModuleSummary>> GetUserModulesAsync(string userId, string orgId)
        {
            var userAccessList = await _userSecurityService.GetRoleAccessForUserAsync(userId, orgId);
            var modules = await _moduleRepo.GetModulesForOrgAndPublicAsyncAsync(orgId);

            var userRoles = await _userSecurityService.GetRolesForUserAsync(userId, orgId);
            if (userRoles.Any(rol => rol.Key == DefaultRoleList.OWNER))
            {
#if VERBOSE
                _adminLogger.Trace($"[UserAccessmanager__GetUserModules] - User is owner will return all ({modules.Count}) modules");
#endif
                return modules;
            }

            var userModules = new List<ModuleSummary>();

            userModules.AddRange(modules.Where(mod => !mod.RestrictByDefault));

            foreach (var access in userAccessList)
            {
                if (access.Read != -1 && !userModules.Any(mod => mod.Id == access.Module.Id))
                {
                    var module = modules.Single(mod => mod.Id == access.Module.Id);
                    userModules.Add(module);
                }

                if (access.Read == -1)
                {
                    userModules.RemoveAll(mod => mod.Id == access.Module.Id);
                }
            }

#if VERBOSE
            _adminLogger.Trace($"[UserAccessmanager__GetUserModules] - Found ({modules.Count}) modules for user.");
#endif

            return userModules.OrderBy(mod => mod.SortOrder).ToList();
        }
        public async Task<Module> GetUserModuleAsync(string moduleKey, string userId, string orgId)
        {
            var module = await _moduleRepo.GetModuleByKeyAsync(moduleKey);
            if (module == null)
                throw new ArgumentException($"Invalid Module: {moduleKey}");


            var roles = await _userSecurityService.GetRolesForUserAsync(userId, orgId);
            if (roles.Any(role => role.Key == DefaultRoleList.OWNER))
            {
                module.UserAccess = UserAccess.GetFullAccess();
                foreach(var area in module.Areas)
                {
                    area.UserAccess = UserAccess.GetFullAccess();
                    foreach(var page in area.Pages)
                    {
                        page.UserAccess = UserAccess.GetFullAccess();
                        foreach(var feature in page.Features)
                        {
                            feature.UserAccess = UserAccess.GetFullAccess();
                        }
                    }

                    foreach(var feature in area.Features)
                    {
                        feature.UserAccess = UserAccess.GetFullAccess();
                    }
                }

                foreach(var feature in module.Features)
                {
                    feature.UserAccess = UserAccess.GetFullAccess();
                }

                return module;
            }

            var originalAreas = new List<Area>(module.Areas);
            var userAccessList = await _userSecurityService.GetModuleRoleAccessForUserAsync(module.Id, userId, orgId);
            
            foreach (var area in module.Areas)
            {
                if (!area.RestrictByDefault || userAccessList.Where(ual=>ual.Area?.Id == area.Id).Any())
                {
                    if (module.UserAccess == null)
                        module.UserAccess = new UserAccess() { Read = UserAccess.Grant };

                    area.UserAccess = UserAccess.GetFullAccess();

                    foreach (var page in area.Pages)
                    {
                        if (!page.RestrictByDefault)
                        {
                            if (module.UserAccess == null)
                                module.UserAccess = new UserAccess() { Read = UserAccess.Grant };

                            if (area.UserAccess == null)
                                area.UserAccess = new UserAccess() { Read = UserAccess.Grant };

                            page.UserAccess = UserAccess.GetFullAccess();

                            foreach (var feature in page.Features)
                            {
                                if (!feature.RestrictByDefault)
                                {
                                    if (page.UserAccess == null)
                                        page.UserAccess = new UserAccess() { Read = UserAccess.Grant };

                                    if (area.UserAccess == null)
                                        area.UserAccess = new UserAccess() { Read = UserAccess.Grant };

                                    if (module.UserAccess == null)
                                        module.UserAccess = new UserAccess() { Read = UserAccess.Grant };

                                    feature.UserAccess = UserAccess.GetFullAccess();
                                }
                            }
                        }
                    }
                }
            }


#if VERBOSE
            _adminLogger.Trace($"Evaluating User Access List, Count: {userAccessList.Count} - Module Areas Count: {originalAreas.Count}");
#endif

            foreach (var access in userAccessList)
            {
                if (!EntityHeader.IsNullOrEmpty(access.Feature))
                {
#if VERBOSE
                    _adminLogger.Trace($"\tRole: {access.Role.Text} - Feature: {access.Feature.Text}.");
#endif

                    var area = originalAreas.SingleOrDefault(ara => ara.Key == access.Area.Key);
                    if (area == null)
                        throw new ArgumentNullException($"Could not find area: {access.Area.Text} when granting access to feature.");

                    var page = area.Pages.SingleOrDefault(pge => pge.Key == access.Page.Key);
                    if (page == null)
                        throw new ArgumentNullException($"Could not find page {access.Page.Text}, area: {access.Area.Text} when granting access to feature.");

                    var feature = page.Features.SingleOrDefault(ftr => ftr.Key == access.Feature.Key);
                    if (feature == null)
                        throw new ArgumentNullException($"Could not find feature {access.Feature.Text} in page {access.Page.Text}, area: {access.Area.Text} when granting access to feature.");

                    if (feature.UserAccess == null)
                    {
                        feature.UserAccess = access.ToUserAccess();
#if VERBOSE
                        _adminLogger.Trace($"\t\tAdded: {feature.UserAccess.Create}, {feature.UserAccess.Read}, {feature.UserAccess.Update}, {feature.UserAccess.Delete}.");
#endif
                    }
                    else
                    {
                        feature.UserAccess = access.Merge(feature.UserAccess);
#if VERBOSE
                        _adminLogger.Trace($"\t\tUpdated: {feature.UserAccess.Create}, {feature.UserAccess.Read}, {feature.UserAccess.Update}, {feature.UserAccess.Delete}.");
#endif
                    }

                    if (page.UserAccess == null && feature.UserAccess.Any())
                        page.UserAccess = new UserAccess() { Read = UserAccess.Grant };

                    if (area.UserAccess == null && feature.UserAccess.Any())
                        area.UserAccess = new UserAccess() { Read = UserAccess.Grant };

                    if (module.UserAccess == null && feature.UserAccess.Any())
                        module.UserAccess = new UserAccess() { Read = UserAccess.Grant };
                }
                else if (!EntityHeader.IsNullOrEmpty(access.Page))
                {
#if VERBOSE
                    _adminLogger.Trace($"\tRole: {access.Role.Text} - Page: {access.Page.Text}.");
#endif

                    var area = originalAreas.SingleOrDefault(ara => ara.Key == access.Area.Key);
                    if (area == null)
                        throw new ArgumentNullException($"Could not find area: {access.Area.Text} when granting access to page.");

                    var page = area.Pages.SingleOrDefault(pge => pge.Key == access.Page.Key);
                    if(page == null)
                        throw new ArgumentNullException($"Could not find page {access.Page.Text}, area: {access.Area.Text} when granting access to page.");

                    if (page.UserAccess == null)
                    {
                        page.UserAccess = access.ToUserAccess();
#if VERBOSE
                        _adminLogger.Trace($"\t\tAdded: {page.UserAccess.Create}, {page.UserAccess.Read}, {page.UserAccess.Update}, {page.UserAccess.Delete}.");
#endif
                    }
                    else
                    {
                        page.UserAccess = access.Merge(page.UserAccess);
#if VERBOSE
                        _adminLogger.Trace($"\t\tUpdated: {page.UserAccess.Create}, {page.UserAccess.Read}, {page.UserAccess.Update}, {page.UserAccess.Delete}.");
#endif
                    }

                    if (area.UserAccess == null && page.UserAccess.Any())
                        area.UserAccess = new UserAccess() { Read = UserAccess.Grant };

                    if (module.UserAccess == null && page.UserAccess.Any())
                        module.UserAccess = new UserAccess() { Read = UserAccess.Grant };

                }
                else if (!EntityHeader.IsNullOrEmpty(access.Area))
                {
#if VERBOSE
                    _adminLogger.Trace($"\tRole: {access.Role.Text} - Area: {access.Area.Text}.");
#endif

                    var area = module.Areas.SingleOrDefault(ara => ara.Key == access.Area.Key);
                    if (area == null)
                        throw new ArgumentNullException($"Could not find area: {access.Area.Text} when granting access to Area.");

                    if (area.UserAccess == null)
                    {
                        area.UserAccess = access.ToUserAccess();

#if VERBOSE
                        _adminLogger.Trace($"\t\tAdded: {area.UserAccess.Create}, {area.UserAccess.Read}, {area.UserAccess.Update}, {area.UserAccess.Delete}.");
#endif
                    }
                    else
                    {
                        area.UserAccess = access.Merge(area.UserAccess);
#if VERBOSE
                        _adminLogger.Trace($"\t\tUpdated: {area.UserAccess.Create}, {area.UserAccess.Read}, {area.UserAccess.Update}, {area.UserAccess.Delete}.");
#endif
                    }

                    if(module.UserAccess == null && area.UserAccess.Any())
                        module.UserAccess = new UserAccess() { Read = UserAccess.Grant };
                }
                else
                {
#if VERBOSE
                    _adminLogger.Trace($"\tRole: {access.Role.Text} - Module: {access.Module.Text}.");
#endif

                    if (module.UserAccess == null)
                    {
                        module.UserAccess = access.ToUserAccess();
#if VERBOSE
                        _adminLogger.Trace($"\t\tAdded: {module.UserAccess.Create}, {module.UserAccess.Read}, {module.UserAccess.Update}, {module.UserAccess.Delete}.");
#endif
                    }
                    else
                    {
                        module.UserAccess = access.Merge(module.UserAccess);
#if VERBOSE
                        _adminLogger.Trace($"\t\tUpdated {module.UserAccess.Create}, {module.UserAccess.Read}, {module.UserAccess.Update}, {module.UserAccess.Delete}.");
#endif
                    }
                }
            }

       

            module.Areas.RemoveAll(mod => mod.UserAccess == null || !mod.UserAccess.Any());
            foreach (var area in module.Areas)
            {
                area.Pages.RemoveAll(page => page.UserAccess == null || !page.UserAccess.Any());
                                
                foreach (var page in area.Pages)
                    page.Features.RemoveAll(feature => feature.UserAccess == null || !feature.UserAccess.Any());
            }

            if (module.UserAccess == null)
            {
                module.UserAccess = UserAccess.None();
            }

            module.UserAccess.RevokeNotSet();
            foreach(var area in module.Areas)
            {
                area.UserAccess.RevokeNotSet();
                foreach(var page in area.Pages)
                {
                    page.UserAccess.RevokeNotSet();
                    foreach(var feature in page.Features)
                    {
                        feature.UserAccess.RevokeNotSet();
                    }
                }
            }        
            
            return module;
        }

    }
}
