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
using Xunit;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Repos.Apps;

namespace LagoVista.UserAdmin.Tests.TokenTests
{
    public class AuthTokenTests
    {
        Mock<IUserManager> _userManager;
        Mock<ISignInManager> _signInManager;
        Mock<IRefreshTokenManager> _refreshTokenManager;
        Mock<IClaimsFactory> _claimsFactory;
        AuthTokenManager _authTokenManager;


        private void Init()
        {
            _signInManager = new Mock<ISignInManager>();
            _userManager = new Mock<IUserManager>();
            _refreshTokenManager = new Mock<IRefreshTokenManager>();
            _claimsFactory = new Mock<IClaimsFactory>();

            var tokenOptions = new TokenAuthOptions()
            {
                AccessExpiration = TimeSpan.FromMinutes(90),
                RefreshExpiration = TimeSpan.FromDays(90),
            };

            _authTokenManager = new AuthTokenManager(tokenOptions, new Mock<IAppInstanceRepo>().Object, _refreshTokenManager.Object, new Mock<IAdminLogger>().Object, _claimsFactory.Object, _signInManager.Object, _userManager.Object);
            _signInManager.Setup(sim => sim.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(Task.FromResult(SignInResult.Success));
            _userManager.Setup(usm => usm.FindByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(new AppUser() { Id = Guid.NewGuid().ToId() }));
            _userManager.Setup(usm => usm.FindByNameAsync(It.IsAny<string>())).Returns(Task.FromResult(new AppUser() { Id = Guid.NewGuid().ToId() }));
            _claimsFactory.Setup(cfa => cfa.GetClaims(It.IsAny<AppUser>())).Returns(new System.Collections.Generic.List<System.Security.Claims.Claim>()
            {
                new System.Security.Claims.Claim("claim1","foo")
            });
        }

        [Fact]
        public async Task ShouildFailOnNull()
        {
            Init();

            var result = await _authTokenManager.AuthAsync(null);
            Assert.False(result.Successful);
        }

        [Fact]
        public async Task ShouldGetToken()
        {
            Init();

            String userId = Guid.NewGuid().ToId();

            var authRequest = new AuthRequest()
            {
                GrantType = "password",
                AppInstanceId = "dontcare",
                AppId = "dontcare",
                ClientType = "iphone",
                Email = "fred@bedrockquery.com",
                Password = "BedRocks!"
            };

            _refreshTokenManager.Setup(rtm => rtm.GenerateRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(
                InvokeResult<RefreshToken>.Create(
                new RefreshToken(userId)
                {

                })
                ));


            var result = await _authTokenManager.AuthAsync(authRequest);
            Assert.True(result.Successful);
        }
    }
}
