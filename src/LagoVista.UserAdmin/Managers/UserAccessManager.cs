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
            if (userRoles.Any(rol => rol.Key == DefaultRoleList.OWNER))
            {
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

                if(access.Read == -1)
                {
                    userModules.RemoveAll(mod => mod.Id == access.Module.Id);
                }
            }

            return userModules.OrderBy(mod=>mod.SortOrder).ToList();
        }
        public async Task<Module> GetUserModuleAsync(string moduleKey, string userId, string orgId)
        {
            var module = await _moduleRepo.GetModuleByKeyAsync(moduleKey);

            var originalAreas = new List<Area>(module.Areas);

            System.Console.WriteLine($"\tModule: {module.Key}");

            var userAccessList = await _userSecurityService.GetModuleRoleAccessForUserAsync(module.Id, userId, orgId);
           
            foreach (var area in module.Areas)
            {
                System.Console.WriteLine($"\t\tArea: {area.Key}");
                if (!area.RestrictByDefault)
                {
                    System.Console.WriteLine($"\t\t- Not Restricted.");
                    area.UserAccess = UserAccess.GetFullAccess();

                    foreach (var page in area.Pages)
                    {
                        System.Console.WriteLine($"\t\t\tPage: {page.Key}");

                        if (!page.RestrictByDefault)
                        {
                            System.Console.WriteLine($"\t\t\t - Not Restricted.");

                            page.UserAccess = UserAccess.GetFullAccess();
                            foreach (var feature in page.Features)
                            {
                                if (!feature.RestrictByDefault)
                                {
                                    System.Console.WriteLine($"\t\t\t\t - Not Restricted.");
                                    feature.UserAccess = UserAccess.GetFullAccess();
                                }
                                else
                                {
                                    feature.UserAccess = UserAccess.None();
                                    System.Console.WriteLine($"\t\t\t\t - Restricted.");
                                }
                            }
                        }
                        else
                        {
                            System.Console.WriteLine($"\t\t\t - Restricted.");
                            page.UserAccess = UserAccess.None();
                        }
                    }
                }
                else
                {
                    System.Console.WriteLine($"\t\t - Restricted.");
                    area.UserAccess = UserAccess.None();
                }
            }



            // Remove anything where there isn't default access.  
            // role specfiic access will be added below.
            module.Areas.RemoveAll(mod => !mod.UserAccess.Any());
            foreach (var area in module.Areas)
            {
                area.Pages.RemoveAll(area => !area.UserAccess.Any());
                foreach (var page in area.Pages)
                    page.Features.RemoveAll(feature => !feature.UserAccess.Any());
            }


            foreach (var access in userAccessList)
            {

                if (!EntityHeader.IsNullOrEmpty(access.Feature))
                {
                    System.Console.WriteLine($"\tRole: {access.Role.Text} - Feature: {access.Feature.Text}.");

                    var area = originalAreas.Single(ara => ara.Id == access.Area.Id);
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

                        System.Console.WriteLine($"\t\tAdded: {feature.UserAccess.Create}, {feature.UserAccess.Read}, {feature.UserAccess.Update}, {feature.UserAccess.Delete}.");
                    }
                    else
                    {
                        feature.UserAccess.Create = access.Create == -1 || feature.UserAccess.Create == -1 ? -1 : access.Create == 0 ? feature.UserAccess.Create : 1;
                        feature.UserAccess.Read = access.Read == -1 || feature.UserAccess.Read == -1 ? -1 : access.Read == 0 ? page.UserAccess.Read : 1;
                        feature.UserAccess.Update = access.Update == -1 || feature.UserAccess.Update == -1 ? -1 : access.Update == 0 ? feature.UserAccess.Update : 1;
                        feature.UserAccess.Delete = access.Delete == -1 || feature.UserAccess.Delete == -1 ? -1 : access.Delete == 0 ? feature.UserAccess.Delete : 1;

                        System.Console.WriteLine($"\t\tUpdated: {feature.UserAccess.Create}, {feature.UserAccess.Read}, {feature.UserAccess.Update}, {feature.UserAccess.Delete}.");
                    }


                }
                else if (!EntityHeader.IsNullOrEmpty(access.Page))
                {
                    System.Console.WriteLine($"\tRole: {access.Role.Text} - Page: {access.Page.Text}.");

                    var area = originalAreas.Single(ara => ara.Id == access.Area.Id);
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

                        System.Console.WriteLine($"\t\tAdded: {page.UserAccess.Create}, {page.UserAccess.Read}, {page.UserAccess.Update}, {page.Description}.");
                    }
                    else
                    {
                        page.UserAccess.Create = access.Create == -1 || page.UserAccess.Create == -1 ? -1 : access.Create == 0 ? page.UserAccess.Create : 1;
                        page.UserAccess.Read = access.Read == -1 || page.UserAccess.Read == -1 ? -1 : access.Read == 0 ? page.UserAccess.Read : 1;
                        page.UserAccess.Update = access.Update == -1 || page.UserAccess.Update == -1 ? -1 : access.Update == 0 ? page.UserAccess.Update : 1;
                        page.UserAccess.Delete = access.Delete == -1 || page.UserAccess.Delete == -1 ? -1 : access.Delete == 0 ? page.UserAccess.Delete : 1;

                        System.Console.WriteLine($"\t\tUpdated: {page.UserAccess.Create}, {page.UserAccess.Read}, {page.UserAccess.Update}, {page.UserAccess.Delete}.");
                    }
                }
                else if (!EntityHeader.IsNullOrEmpty(access.Area))
                {
                    System.Console.WriteLine($"\tRole: {access.Role.Text} - Area: {access.Area.Text}.");

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

                        System.Console.WriteLine($"\t\tAdded: {area.UserAccess.Create}, {area.UserAccess.Read}, {area.UserAccess.Update}, {area.Description}.");
                    }
                    else
                    {
                        area.UserAccess.Create = access.Create == -1 || area.UserAccess.Create == -1 ? -1 : access.Create == 0 ? area.UserAccess.Create : 1;
                        area.UserAccess.Read = access.Read == -1 || area.UserAccess.Read == -1 ? -1 : access.Read == 0 ? area.UserAccess.Read : 1;
                        area.UserAccess.Update = access.Update == -1 || area.UserAccess.Update == -1 ? -1 : access.Update == 0 ? area.UserAccess.Update : 1;
                        area.UserAccess.Delete = access.Delete == -1 || area.UserAccess.Delete == -1 ? -1 : access.Delete == 0 ? area.UserAccess.Delete : 1;

                        System.Console.WriteLine($"\t\tUpdated: {area.UserAccess.Create}, {area.UserAccess.Read}, {area.UserAccess.Update}, {area.UserAccess.Delete}.");
                    }
                }
                else
                {
                    System.Console.WriteLine($"\tRole: {access.Role.Text} - Module: {access.Module.Text}.");

                    if (module.UserAccess == null)
                    {
                        module.UserAccess = new UserAccess()
                        {
                            Create = access.Create,
                            Read = access.Read,
                            Update = access.Update,
                            Delete = access.Delete
                        };

                        System.Console.WriteLine($"\t\tAdded: {module.UserAccess.Create}, {module.UserAccess.Read}, {module.UserAccess.Update}, {module.UserAccess.Delete}.");
                    }
                    else
                    {
                        module.UserAccess.Create = access.Create == -1 || module.UserAccess.Create == -1 ? -1 : access.Create == 0 ? module.UserAccess.Create : 1;
                        module.UserAccess.Read = access.Read == -1 || module.UserAccess.Read == -1 ? -1 : access.Read == 0 ? module.UserAccess.Read : 1;
                        module.UserAccess.Update = access.Update == -1 || module.UserAccess.Update == -1 ? -1 : access.Update == 0 ? module.UserAccess.Update : 1;
                        module.UserAccess.Delete = access.Delete == -1 || module.UserAccess.Delete == -1 ? -1 : access.Delete == 0 ? module.UserAccess.Delete : 1;
                        System.Console.WriteLine($"\t\tUpdated {module.UserAccess.Create}, {module.UserAccess.Read}, {module.UserAccess.Update}, {module.UserAccess.Delete}.");
                    }
                }
            }

            module.Areas.RemoveAll(mod => !mod.UserAccess.Any());
            foreach (var area in module.Areas)
            {
                area.Pages.RemoveAll(area => !area.UserAccess.Any());
                foreach (var page in area.Pages)
                    page.Features.RemoveAll(feature => !feature.UserAccess.Any());
            }

            return module;
        }

        private int Calculate(int? existingLevel, int? newLevel)
        {

            return newLevel.Value;
        }
    }
}
