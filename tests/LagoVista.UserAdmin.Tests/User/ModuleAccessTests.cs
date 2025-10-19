// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 873b7298fab2197454ffc9c44d6af2043c569c08d6749bc656db659a05f1f974
// IndexVersion: 0
// --- END CODE INDEX META ---
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
