// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 913b80574729bec4b2828401c6835e4163a1fca11139fb88801b0dd7788966d8
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.AspNetCore.Identity.Models;
using LagoVista.Core.Authentication.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using LagoVista.Core;
using System.Text;
using System.Threading.Tasks;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Repos.Apps;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Interfaces.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LagoVista.UserAdmin.Models.Apps;
using System.Collections.Generic;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;

namespace LagoVista.UserAdmin.Tests.TokenTests
{
    [TestClass]
    public class AuthTokenTests
    {
        Mock<IUserManager> _userManager;
        Mock<ISignInManager> _signInManager;
        Mock<IRefreshTokenManager> _refreshTokenManager;
        Mock<ITokenHelper> _tokenHelper;
        Mock<IOrganizationManager> _orgManager;
        Mock<IAppInstanceManager> _appInstanceManager;
        Mock<IAuthRequestValidators> _authRequestValidators;

        AuthTokenManager _authTokenManager;
        const string ORG_ID = "45C9DBB7B80E453890F9DF579DD7EB11";

        [TestInitialize]
        public void Init()
        {
            _signInManager = new Mock<ISignInManager>();
            _userManager = new Mock<IUserManager>();
            _refreshTokenManager = new Mock<IRefreshTokenManager>();
            _tokenHelper = new Mock<ITokenHelper>();
            _orgManager = new Mock<IOrganizationManager>();
            _appInstanceManager = new Mock<IAppInstanceManager>();
            _authRequestValidators = new Mock<IAuthRequestValidators>();

            var tokenOptions = new TokenAuthOptions()
            {
                AccessExpiration = TimeSpan.FromMinutes(90),
                RefreshExpiration = TimeSpan.FromDays(90),
            };
          
            _authTokenManager = new AuthTokenManager(new Mock<IAppInstanceRepo>().Object, new Mock<IAuthenticationLogManager>().Object, new Mock<ISingleUseTokenManager>().Object, _orgManager.Object, _refreshTokenManager.Object,
                _authRequestValidators.Object, new Mock<IOrganizationRepo>().Object, _tokenHelper.Object, _appInstanceManager.Object,
                new Mock<IAdminLogger>().Object, _signInManager.Object, _userManager.Object);

            _appInstanceManager.Setup(ais => ais.UpdateLastLoginAsync(It.IsAny<string>(), It.IsAny<AuthRequest>())).ReturnsAsync(InvokeResult<AppInstance>.Create(new AppInstance("rowid", "userid")));
            _appInstanceManager.Setup(ais => ais.UpdateLastAccessTokenRefreshAsync(It.IsAny<string>(), It.IsAny<AuthRequest>())).ReturnsAsync(InvokeResult<AppInstance>.Create(new AppInstance("rowid", "userid")));

            _orgManager.Setup(orm => orm.GetOrganizationsForUserAsync(It.IsAny<string>(), It.IsAny<EntityHeader>(), It.IsAny<EntityHeader>())).ReturnsAsync(new List<OrgUser>()
            {
                new OrgUser(ORG_ID,Guid.NewGuid().ToId())
            });

            _signInManager.Setup(sim => sim.PasswordSignInAsync(It.IsAny<AuthLoginRequest>())).Returns(Task.FromResult(InvokeResult<UserLoginResponse>.Create(new UserLoginResponse())));
            _userManager.Setup(usm => usm.FindByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(new AppUser() { Id = Guid.NewGuid().ToId(), CurrentOrganization = new OrganizationSummary() { Id = ORG_ID, Name = "dontcare", Text = "dontcare" } }));
            _userManager.Setup(usm => usm.FindByNameAsync(It.IsAny<string>())).Returns(Task.FromResult(new AppUser() { CurrentOrganization = new OrganizationSummary() { Id = ORG_ID, Name = "dontcare", Text = "dontcare" } }));
            _refreshTokenManager.Setup(rtm => rtm.GenerateRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task<RefreshToken>.FromResult(InvokeResult<RefreshToken>.Create(new RefreshToken("XXXX"))));
            _authRequestValidators.Setup(arv => arv.ValidateAuthRequest(It.IsAny<AuthRequest>())).Returns(InvokeResult.Success);
            _authRequestValidators.Setup(arv => arv.ValidateAccessTokenGrant(It.IsAny<AuthRequest>())).Returns(InvokeResult.Success);
            _authRequestValidators.Setup(arv => arv.ValidateRefreshTokenGrant(It.IsAny<AuthRequest>())).Returns(InvokeResult.Success);
            _tokenHelper.Setup(tlp => tlp.GenerateAuthResponseAsync(It.IsAny<AppUser>(), It.IsAny<AuthRequest>(), It.IsAny<InvokeResult<RefreshToken>>())).ReturnsAsync(new InvokeResult<AuthResponse>()
            {
                Result = new AuthResponse()
                {
                    AccessToken = "ACC",
                    AccessTokenExpiresUTC = DateTime.Now.AddMinutes(30).ToJSONString(),
                }
            });
        }

        //TODO: SHould write some tests here but behind schedule...did deskcheck of code and after refactoring its very straight forward.


        [TestMethod]
        public async Task ShouldGenerateAccessToken()
        {
            var request = new AuthRequest()
            {
                AppId = "APP123",
                AppInstanceId = "INST12",
                ClientType = "APP",
                DeviceId = "DEV001",
                Email = "email@address.net",
                GrantType = "password",
                OrgId = ORG_ID,
                OrgName = "orgname",
                Password = "pwd",
                UserName = "email@foo.net"

            };

            var result = await _authTokenManager.AccessTokenGrantAsync(request);
            foreach (var err in result.Errors)
            {
                Console.WriteLine(err.Message);
            }
            Assert.IsTrue(result.Successful);
        }


        [TestMethod]
        public async Task ShouldGenerateRefreshToken()
        {
            var request = new AuthRequest()
            {
                AppId = "APP123",
                AppInstanceId = "INST12",
                ClientType = "APP",
                DeviceId = "DEV001",
                Email = "email@address.net",
                GrantType = "refreshtoken",
                OrgId = ORG_ID,
                OrgName = "orgname",
                Password = "pwd",
                UserName = "email@foo.net"

            };

            var result = await _authTokenManager.RefreshTokenGrantAsync(request);
            foreach (var err in result.Errors)
            {
                Console.WriteLine(err.Message);
            }
            Assert.IsTrue(result.Successful);
        }
    }
}
