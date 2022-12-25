using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Tests.User
{
    [TestClass]
    public class ModuleAccessTests : SecurityBase
    {
        [TestMethod]
        public async Task Should_Get_Non_Restricted_Module()
        {
            _mod5.RestrictByDefault = false;
            
            var modules = await AccessManager.GetUserModulesAsync(USER_ID, ORG_ID);

            AssertContainsModule(modules, _mod5);
            AssertDoesNotContainsModule(modules, _mod1);
            AssertDoesNotContainsModule(modules, _mod2);
            AssertDoesNotContainsModule(modules, _mod3);
            AssertDoesNotContainsModule(modules, _mod4);
        }

        [TestMethod]
        public async Task Should_Get_Role_Added_Module()
        {
            _mod5.RestrictByDefault = false;
          
            AddModuleAccess(_role1, _mod1);

            var modules = await AccessManager.GetUserModulesAsync(USER_ID, ORG_ID);
            AssertContainsModule(modules, _mod5);
            AssertContainsModule(modules, _mod1);
            AssertDoesNotContainsModule(modules, _mod2);
            AssertDoesNotContainsModule(modules, _mod3);
            AssertDoesNotContainsModule(modules, _mod4);
        }

        [TestMethod]
        public async Task Should_Remove_Non_Restricted_When_Disabled()
        {
            _mod5.RestrictByDefault = false;
            
            AddModuleAccess(_role1, _mod5, -1);

            var modules = await AccessManager.GetUserModulesAsync(USER_ID, ORG_ID);
            AssertDoesNotContainsModule(modules, _mod1);
            AssertDoesNotContainsModule(modules, _mod2);
            AssertDoesNotContainsModule(modules, _mod3);
            AssertDoesNotContainsModule(modules, _mod4);
            AssertDoesNotContainsModule(modules, _mod5);
        }
    }
}
