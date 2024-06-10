using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.Logging.Utils;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Tests
{
    public class SecurityBase
    {
        UserAccessManager _accessManager;
        Mock<IRoleRepo> _roleRepo = new Mock<IRoleRepo>();
        Mock<IOrganizationRepo> _orgRepo = new Mock<IOrganizationRepo>();
        Mock<IUserSecurityServices> _userSecurityService = new Mock<IUserSecurityServices>();
        IModuleRepo _moduleRepo = new InMemoryModuleRepo();
        Mock<IRoleAccessRepo> _roleAccessRepo = new Mock<IRoleAccessRepo>();

        List<Models.Security.RoleAccess> _roleAccess = new List<RoleAccess>();

        protected const string USER_ID = "9C33C709A60B4D539CB8031DA653B1BD";
        protected const string ORG_ID = "2D04F767534A40F0BC40E15FF2804681";

        List<Role> _roles;

        List<Module> _modules = new List<Module>();

        protected Module _mod1;
        protected Module _mod2;
        protected Module _mod3;
        protected Module _mod4;
        protected Module _mod5;

        protected Role _role1;
        protected Role _role2;
        protected Role _role3;
        protected Role _role4;
        protected Role _role5;

        Module AddModule(string key, bool restrictByDefault = false, int sortOrder = 0)
        {
            return new Module()
            {
                Key = key,
                Id = key,
                Name = key,
                SortOrder = sortOrder,
                RestrictByDefault = restrictByDefault
            };
        }

        Area AddArea(Module mod, string key)
        {
            var area = new Area()
            {
                Pages = new List<Page>(),
                Key = key,
                Id = key,
                Name = key,
                RestrictByDefault = true,
            };

            mod.Areas.Add(area);

            return area;
        }

        Page AddPage(Area area, string pageKey)
        {
            var page = new Page()
            {
                Features = new List<Feature>(),
                Key = pageKey,
                Id = pageKey,
                Name = pageKey,
                RestrictByDefault = true,
            };

            area.Pages.Add(page);

            return page;
        }

        void AddFeature(Page page, string featureKey)
        {
            var feature = new Feature()
            {
                Key = featureKey,
                Id = featureKey,
                Name = featureKey,
                RestrictByDefault = true,
            };

            page.Features.Add(feature);
        }

        private void PrintModule(Module mod)
        {
            Console.WriteLine(mod.Name);
            foreach (var area in mod.Areas)
            {
                Console.WriteLine($"\t{area.Name}");
                foreach (var page in area.Pages)
                {
                    Console.WriteLine($"\t\t{page.Name}");
                    foreach (var feature in page.Features)
                        Console.WriteLine($"\t\t\t{feature.Name}");
                }
            }
        }

        public Role CreateRole(string key)
        {
            return new Role()
            {
                Key = key,
                Id = key,
                Name = key
            };
        }

        private class InMemoryModuleRepo : IModuleRepo
        {
            private List<Module> _modules = new List<Module>();

            public Task AddModuleAsync(Module module)
            {
                _modules.Add(module);
                return Task.CompletedTask;
            }

            public Task DeleteModuleAsync(string id)
            {
                throw new NotImplementedException();
            }

            public Task<ListResponse<ModuleSummary>> GetAllModulesAsync(string orgId, ListRequest listRequest)
            {
                return Task.FromResult(ListResponse<ModuleSummary>.Create( _modules.Select(mod => mod.CreateSummary()).OrderBy(mod => mod.SortOrder).ToList()));
            }

            public Task<Module> GetModuleAsync(string id)
            {
                return Task.FromResult(Clone(_modules.SingleOrDefault(mod => mod.Id == id)));
            }

            public Task<Module> GetModuleByKeyAsync(string key)
            {
                return Task.FromResult(Clone(_modules.SingleOrDefault(mod => mod.Key == key)));
            }

            public Task<Module> GetModuleByKeyAsync(string key, string orgId)
            {
                return Task.FromResult(_modules.SingleOrDefault(mod => mod.Key == key && mod.OwnerOrganization.Id == orgId));
            }

            public Task<List<ModuleSummary>> GetModulesForOrgAndPublicAsyncAsync(string orgId)
            {
                return Task.FromResult(_modules.Select(mod => mod.CreateSummary()).Where(mod=> mod.IsPublic || mod.OwnerOrgId == orgId).OrderBy(mod => mod.SortOrder).ToList());
            }

            public Task UpdateModuleAsync(Module module)
            {
                return Task.CompletedTask;
            }

            private Module Clone(Module module)
            {
                var json = JsonConvert.SerializeObject(module);
                return JsonConvert.DeserializeObject<Module>(json);
            }
        }

        [TestInitialize]
        public void Init()
        {
            _accessManager = new UserAccessManager(_userSecurityService.Object, _orgRepo.Object, _moduleRepo,  new AdminLogger(new ConsoleLogWriter()));

            _mod1 = AddModule("mod1_restrict", true, 1);
            _mod2 = AddModule("mod2_restrict", true, 2);
            _mod3 = AddModule("mod3_restrict", true, 3);
            _mod4 = AddModule("mod4_restrict", true, 4);
            _mod5 = AddModule("mod5_non_restrict", false, 5);
            _modules = new List<Module>() { _mod1, _mod2, _mod3, _mod4, _mod5 };

            var area = AddArea(_mod1, "area1");
            var page = AddPage(area, $"{area.Key}_page1");
            AddFeature(page, $"{area.Key}_{page.Key}_f1");
            AddFeature(page, $"{area.Key}_{page.Key}_f2");
            page = AddPage(area, $"{area.Key}_page2");
            AddFeature(page, $"{area.Key}_{page.Key}_f1");
            page = AddPage(area, $"{area.Key}_page3");
            AddFeature(page, $"{area.Key}_{page.Key}_f1");
            page = AddPage(area, $"{area.Key}_page4");
            AddFeature(page, $"{area.Key}_{page.Key}_f1");
            AddFeature(page, $"{area.Key}_{page.Key}_f2");

            area = AddArea(_mod1, "area2");
            page = AddPage(area, $"{area.Key}_page1");
            page = AddPage(area, $"{area.Key}_page2");
            AddFeature(page, $"{area.Key}_{page.Key}_f1");
            page = AddPage(area, $"{area.Key}_page3");
            AddFeature(page, $"{area.Key}_{page.Key}_f1");
            AddFeature(page, $"{area.Key}_{page.Key}_f2");
            page = AddPage(area, $"{area.Key}_page4`");
            page = AddPage(area, $"{area.Key}_page5`");
            AddFeature(page, $"{area.Key}_{page.Key}_f1");
            page = AddPage(area, $"{area.Key}_page6");
            AddFeature(page, $"{area.Key}_{page.Key}_f1");
            page = AddPage(area, $"{area.Key}_page7`");
            AddFeature(page, $"{area.Key}_{page.Key}_f1");
            AddFeature(page, $"{area.Key}_{page.Key}_f2");
            AddFeature(page, $"{area.Key}_{page.Key}_f3");
            AddFeature(page, $"{area.Key}_{page.Key}_f4");

            area = AddArea(_mod1, "area3");
            page = AddPage(area, $"{area.Key}_page1");
            AddFeature(page, $"{area.Key}_{page.Key}_f1");
            AddFeature(page, $"{area.Key}_{page.Key}_f2");
            page = AddPage(area, $"{area.Key}_page2");
            AddFeature(page, $"{area.Key}_{page.Key}_f1");

            area = AddArea(_mod1, "area4");
            page = AddPage(area, $"{area.Key}_page1");
            AddFeature(page, $"{area.Key}_{page.Key}_f1");
            page = AddPage(area, $"{area.Key}_page2");
            AddFeature(page, $"{area.Key}_{page.Key}_f1");
            AddFeature(page, $"{area.Key}_{page.Key}_f2");

            area = AddArea(_mod1, "area5");
            page = AddPage(area, $"{area.Key}_page1");
            AddFeature(page, $"{area.Key}_{page.Key}_f1");
            AddFeature(page, $"{area.Key}_{page.Key}_f2");
            page = AddPage(area, $"{area.Key}_page2");
            AddFeature(page, $"{area.Key}_{page.Key}_f1");
            AddFeature(page, $"{area.Key}_{page.Key}_f2");

            _role1 = CreateRole("role1");
            _role2 = CreateRole("role2");
            _role3 = CreateRole("role3");
            _role4 = CreateRole("role4");
            _role5 = CreateRole("role5");

            _moduleRepo.AddModuleAsync(_mod1);
            _moduleRepo.AddModuleAsync(_mod2);
            _moduleRepo.AddModuleAsync(_mod3);
            _moduleRepo.AddModuleAsync(_mod4);
            _moduleRepo.AddModuleAsync(_mod5);

            _userSecurityService.Setup(uss => uss.GetRoleAccessForUserAsync(USER_ID, ORG_ID)).ReturnsAsync(new List<Models.Security.RoleAccess>());

            _roles = new List<Role>();
            _userSecurityService.Setup(uss => uss.GetRolesForUserAsync(USER_ID, ORG_ID)).ReturnsAsync(_roles);

            _roleAccess = new List<RoleAccess>();
            
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod1.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod1.Id).ToList());
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod2.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod2.Id).ToList());
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod3.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod3.Id).ToList());
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod4.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod4.Id).ToList());
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod5.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod5.Id).ToList());

            _roleRepo.Setup(rol => rol.GetRoleByKeyAsync(_role1.Key, It.IsAny<string>())).ReturnsAsync(_role1);
            _roleRepo.Setup(rol => rol.GetRoleByKeyAsync(_role2.Key, It.IsAny<string>())).ReturnsAsync(_role2);
            _roleRepo.Setup(rol => rol.GetRoleByKeyAsync(_role3.Key, It.IsAny<string>())).ReturnsAsync(_role3);
            _roleRepo.Setup(rol => rol.GetRoleByKeyAsync(_role4.Key, It.IsAny<string>())).ReturnsAsync(_role4);
            _roleRepo.Setup(rol => rol.GetRoleByKeyAsync(_role5.Key, It.IsAny<string>())).ReturnsAsync(_role5);
        }

        protected void AddModuleAccess(Role role, Module module, int read = 1, int create = 0, int update = 0, int delete = 0)
        {
            _roleAccess.Add(new Models.Security.RoleAccess()
            {
                Role = role.ToEntityHeader(),
                Module = module.ToEntityHeader(),
                Create = create,
                Read = read,
                Update = update,
                Delete = delete
            });

            _userSecurityService.Setup(uss => uss.GetRoleAccessForUserAsync(USER_ID, ORG_ID)).ReturnsAsync(_roleAccess);
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod1.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod1.Id).ToList());
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod2.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod2.Id).ToList());
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod3.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod3.Id).ToList());
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod4.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod4.Id).ToList());
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod5.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod5.Id).ToList());
        }

        protected void AddAreaAccess(Role role, Module module, Area area, int read = 1, int create = 0, int update = 0, int delete = 0)
        {
            _roleAccess.Add(new Models.Security.RoleAccess()
            {
                Role = role.ToEntityHeader(),
                Module = module.ToEntityHeader(),
                Area = area.ToEntityHeader(),
                Create = create,
                Read = read,
                Update = update,
                Delete = delete
            });

            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod1.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod1.Id).ToList());
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod2.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod2.Id).ToList());
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod3.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod3.Id).ToList());
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod4.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod4.Id).ToList());
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod5.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod5.Id).ToList());
        }

        protected void AddPageAccess(Role role, Module module, Area area, Page page, int read = 1, int create = 0, int update = 0, int delete = 0)
        {
            _roleAccess.Add(new Models.Security.RoleAccess()
            {
                Role = role.ToEntityHeader(),
                Module = module.ToEntityHeader(),
                Area = area.ToEntityHeader(),
                Page = page.ToEntityHeader(),
                Create = create,
                Read = read,
                Update = update,
                Delete = delete
            });

            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod1.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod1.Id).ToList());
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod2.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod2.Id).ToList());
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod3.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod3.Id).ToList());
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod4.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod4.Id).ToList());
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod5.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod5.Id).ToList());
        }

        protected void AddFeatureAccess(Role role, Module module, Area area, Page page, Feature feature, int read = 1, int create = 0, int update = 0, int delete = 0)
        {
            _roleAccess.Add(new Models.Security.RoleAccess()
            {
                Role = role.ToEntityHeader(),
                Module = module.ToEntityHeader(),
                Area = area.ToEntityHeader(),
                Page = page.ToEntityHeader(),
                Feature = feature.ToEntityHeader(),
                Create = create,
                Read = read,
                Update = update,
                Delete = delete
            });

            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod1.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod1.Id).ToList());
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod2.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod2.Id).ToList());
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod3.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod3.Id).ToList());
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod4.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod4.Id).ToList());
            _userSecurityService.Setup(uss => uss.GetModuleRoleAccessForUserAsync(_mod5.Id, USER_ID, ORG_ID)).ReturnsAsync(_roleAccess.Where(mod => mod.Module.Id == _mod5.Id).ToList());
        }

        protected void AssertContainsModule(IEnumerable<ModuleSummary> modules, Module module)
        {
            Assert.IsNotNull(modules.Single(mod => mod.Key == module.Key));
        }
        protected void AssertDoesNotContainsModule(IEnumerable<ModuleSummary> modules, Module module)
        {
            Assert.IsNull(modules.SingleOrDefault(mod => mod.Key == module.Key));
        }

        protected IIUserAccessManager AccessManager => _accessManager;

        protected void ValidateUserAccess(UserAccess access, int create, int read, int update, int delete)
        {
            Assert.IsNotNull(access);
            Assert.AreEqual(create, access.Create);
            Assert.AreEqual(read, access.Read);
            Assert.AreEqual(update, access.Update);
            Assert.AreEqual(delete, access.Delete);
        }
    }
}
