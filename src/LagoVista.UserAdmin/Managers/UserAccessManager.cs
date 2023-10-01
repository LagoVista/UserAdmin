﻿using LagoVista.Core.Models;
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
            var modules = await _moduleRepo.GetModulesForOrgAndPublicAsyncAsync(orgId);

            var userRoles = await _userSecurityService.GetRolesForUserAsync(userId, orgId);
            if (userRoles.Any(rol => rol.Key == DefaultRoleList.OWNER))
            {
                Console.WriteLine($"[UserAccessmanager__GetUserModules] - User is owner will return all ({modules.Count}) modules");
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

            Console.WriteLine($"[UserAccessmanager__GetUserModules] - Found ({modules.Count}) modules for user.");

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
                if (!area.RestrictByDefault)
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

            System.Console.WriteLine($"------");

            foreach (var access in userAccessList)
            {
                if (!EntityHeader.IsNullOrEmpty(access.Feature))
                {
                    System.Console.WriteLine($"\tRole: {access.Role.Text} - Feature: {access.Feature.Text}.");

                    var area = originalAreas.Single(ara => ara.Id == access.Area.Id);
                    var page = area.Pages.Single(pge => pge.Id == access.Page.Id);
                    var feature = page.Features.Single(ftr => ftr.Id == access.Feature.Id);

                    if (feature.UserAccess == null)
                    {
                        feature.UserAccess = access.ToUserAccess();
                        System.Console.WriteLine($"\t\tAdded: {feature.UserAccess.Create}, {feature.UserAccess.Read}, {feature.UserAccess.Update}, {feature.UserAccess.Delete}.");
                    }
                    else
                    {
                        feature.UserAccess = access.Merge(feature.UserAccess);
                        System.Console.WriteLine($"\t\tUpdated: {feature.UserAccess.Create}, {feature.UserAccess.Read}, {feature.UserAccess.Update}, {feature.UserAccess.Delete}.");
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
                    System.Console.WriteLine($"\tRole: {access.Role.Text} - Page: {access.Page.Text}.");

                    var area = originalAreas.Single(ara => ara.Id == access.Area.Id);
                    var page = area.Pages.Single(pge => pge.Id == access.Page.Id);
                    if (page.UserAccess == null)
                    {
                        page.UserAccess = access.ToUserAccess();
                        System.Console.WriteLine($"\t\tAdded: {page.UserAccess.Create}, {page.UserAccess.Read}, {page.UserAccess.Update}, {page.UserAccess.Delete}.");
                    }
                    else
                    {
                        page.UserAccess = access.Merge(page.UserAccess);
                        System.Console.WriteLine($"\t\tUpdated: {page.UserAccess.Create}, {page.UserAccess.Read}, {page.UserAccess.Update}, {page.UserAccess.Delete}.");
                    }

                    if (area.UserAccess == null && page.UserAccess.Any())
                        area.UserAccess = new UserAccess() { Read = UserAccess.Grant };

                    if (module.UserAccess == null && page.UserAccess.Any())
                        module.UserAccess = new UserAccess() { Read = UserAccess.Grant };

                }
                else if (!EntityHeader.IsNullOrEmpty(access.Area))
                {
                    System.Console.WriteLine($"\tRole: {access.Role.Text} - Area: {access.Area.Text}.");

                    var area = module.Areas.Single(ara => ara.Id == access.Area.Id);
                    if (area.UserAccess == null)
                    {
                        area.UserAccess = access.ToUserAccess();

                        System.Console.WriteLine($"\t\tAdded: {area.UserAccess.Create}, {area.UserAccess.Read}, {area.UserAccess.Update}, {area.UserAccess.Delete}.");
                    }
                    else
                    {
                        area.UserAccess = access.Merge(area.UserAccess);
                        System.Console.WriteLine($"\t\tUpdated: {area.UserAccess.Create}, {area.UserAccess.Read}, {area.UserAccess.Update}, {area.UserAccess.Delete}.");
                    }

                    if(module.UserAccess == null && area.UserAccess.Any())
                        module.UserAccess = new UserAccess() { Read = UserAccess.Grant };
                }
                else
                {
                    System.Console.WriteLine($"\tRole: {access.Role.Text} - Module: {access.Module.Text}.");

                    if (module.UserAccess == null)
                    {
                        module.UserAccess = access.ToUserAccess();

                        System.Console.WriteLine($"\t\tAdded: {module.UserAccess.Create}, {module.UserAccess.Read}, {module.UserAccess.Update}, {module.UserAccess.Delete}.");
                    }
                    else
                    {
                        module.UserAccess = access.Merge(module.UserAccess);
                        System.Console.WriteLine($"\t\tUpdated {module.UserAccess.Create}, {module.UserAccess.Read}, {module.UserAccess.Update}, {module.UserAccess.Delete}.");
                    }
                }
            }

            module.Areas.RemoveAll(mod => mod.UserAccess == null || !mod.UserAccess.Any());
            foreach (var area in module.Areas)
            {
                area.Pages.RemoveAll(area => area.UserAccess == null || !area.UserAccess.Any());
                foreach (var page in area.Pages)
                    page.Features.RemoveAll(feature => feature.UserAccess == null || !feature.UserAccess.Any());
            }

            if(module.UserAccess == null)
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
