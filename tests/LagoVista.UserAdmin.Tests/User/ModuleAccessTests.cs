using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Tests.User
{
    [TestClass]
    public class ModuleAccessTests
    {

        UserAccessManager _accessManager;
        Mock<IRoleRepo> _roleRepo = new Mock<IRoleRepo>();
        Mock<IUserSecurityServices> _userSecurityService = new Mock<IUserSecurityServices>();
        Mock<IModuleRepo> _moduleRepo = new Mock<IModuleRepo>();
        Mock<IRoleAccessRepo> _roleAccessRepo = new Mock<IRoleAccessRepo>();

        const string USER_ID = "9C33C709A60B4D539CB8031DA653B1BD";
        const string ORG_ID = "2D04F767534A40F0BC40E15FF2804681";

        [TestInitialize]
        public void Init()
        {
            _accessManager = new UserAccessManager(_userSecurityService.Object, _moduleRepo.Object);

            _roleRepo.Setup(rol => rol.GetRoleByKeyAsync("role1", "dontcare")).ReturnsAsync(new Models.Users.Role() { Id = "ABC1234" });
            _roleRepo.Setup(rol => rol.GetRoleByKeyAsync("role2", "dontcare")).ReturnsAsync(new Models.Users.Role() { Id = "DEF1234" });
            _roleRepo.Setup(rol => rol.GetRoleByKeyAsync("role3", "dontcare")).ReturnsAsync(new Models.Users.Role() { Id = "GHI1234" });
        }

        [TestMethod]
        public async Task GetModules()
        {
            var modules = await _accessManager.GetUserModulesAsync(USER_ID, ORG_ID);
        }

    }
}
