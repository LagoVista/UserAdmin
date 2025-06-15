using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.Core.Interfaces;
using LagoVista.IoT;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.Logging.Utils;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Repos.Orgs;
using LagoVista.UserAdmin.Repos.Repos.Security;
using LagoVista.UserAdmin.Repos.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Tests.User
{
    [TestClass]
    public class SecurityIntegrationTests
    {
        const string USER_ID = "6144B07693B048E4813F1D860102E9F1";
        const string ORG_ID = "AA2C78499D0140A5A9CE4B7581EF9691";

        IIUserAccessManager _accessMangager;
        IRoleAccessRepo _roleAccessRepo;
        IOrganizationRepo _orgRepo;

        [TestInitialize]
        public void Init()
        {
            var cacheProvider = new Mock<ICacheProvider>().Object;
            var httpContext = new Mock<IHttpContextAccessor>();

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimsFactory.CurrentUserId, USER_ID));
            identity.AddClaim(new Claim(ClaimsFactory.CurrentOrgId, ORG_ID));
            identity.AddClaim(new Claim(ClaimTypes.Role, DefaultRoleList.EXECUTIVE));

            var user = new System.Security.Claims.ClaimsPrincipal(identity);                      
            httpContext.Setup(ctx => ctx.HttpContext.User).Returns(() =>
            {
                return user;
            });
            
            var adminLogger = new AdminLogger(new ConsoleLogWriter());
            _roleAccessRepo = new RoleAccessRepo(new AdminConnectivitySettings(), adminLogger, cacheProvider, new Mock<IAppConfig>().Object);
            _orgRepo = new Mock<IOrganizationRepo>().Object;
            // To run this live, you will need to forward reference SecurityUserService nuget in the AppSupport package, the
            // reference for this is in the test .csproj file but you will likely need to adjust the nuget version.
            //var securityManager = new SecurityUserService(httpContext.Object, new RoleRepo(new AdminConnectivitySettings(), new DefaultRoleList(), adminLogger, cacheProvider), new UserRoleRepo(new AdminConnectivitySettings(), adminLogger), _roleAccessRepo);
            var securityManager = new Mock<IUserSecurityServices>();

            _accessMangager = new UserAccessManager(securityManager.Object, _orgRepo, new ModuleRepo(new AdminConnectivitySettings(), adminLogger, cacheProvider), adminLogger);
        }

        [TestMethod]
        public async Task GetSecurity()
        {
            var moduleKey = "business";

            var module = await _accessMangager.GetUserModuleAsync(moduleKey, USER_ID, ORG_ID );
            Console.WriteLine($"module: {module.Id}");
            //foreach(var area in module.Areas)
            //{
            //    Console.WriteLine(area.NickName);
            //    foreach(var page in area.Pages)
            //    {
            //        Console.WriteLine($"\t{page.CardTitle}");
            //    }
            //}
        }

        [TestMethod]
        public async Task GetRoleAccessForUser()
        {
            var access = await _roleAccessRepo.GetRoleAccessForRoleAsync("ACDC1BADF00D1CAFEF12CE0FF55F2B02", ORG_ID);
            foreach (var result in access)
            {
                Console.WriteLine($"Area {result.Area?.Text} - {result.Page?.Text}  - {result.Module?.Text} - {result.Module?.Id}");
            }

            var results = access.Where(mod => mod.Module.Id == "A1F392D8F25B4AD1BC19661C50B4F4F7");
            foreach(var  result in results)
            {
                Console.WriteLine($"Area {result.Area?.Text} - {result.Page?.Text}");
            }
        }
    }
}
