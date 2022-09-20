using LagoVista.AspNetCore.Identity.Services;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Tests
{
    [TestClass]
    public class TwitterTests
    {
        TwitterAuthServices _twtter;

        [TestInitialize]
        public async Task Init()
        {
            _twtter = new TwitterAuthServices(new OAuthSettings(), new AppConfig());
        }
 
        [TestMethod]
        public async Task BuildSignature()
        {
           var resut = await _twtter.ObtainRequestTokenAsync();
            Console.WriteLine(resut.Token);
        
        }
    
    }

    public class OAuthSettings : IOAuthSettings
    {
        public OAuthConfig FaceBookOAuth => throw new NotImplementedException();

        public OAuthConfig TwitterOAuth => new OAuthConfig("pPR83tTh8k16ilpfQmjOTa3GK", "VgwPDv2oLct7IWIh2Ep19Sx9NhZiDz4dIPqqRnflOmWYuKB0B8");

        public OAuthConfig GitHubOAuth => throw new NotImplementedException();

        public OAuthConfig LinkedInOAuth => throw new NotImplementedException();

        public OAuthConfig MicrosoftOAuth => throw new NotImplementedException();

        public OAuthConfig GoogleOAuth => throw new NotImplementedException();
    }

    public class AppConfig : IAppConfig
    {
        public PlatformTypes PlatformType => throw new NotImplementedException();

        public Environments Environment => throw new NotImplementedException();

        public AuthTypes AuthType => throw new NotImplementedException();

        public EntityHeader SystemOwnerOrg => throw new NotImplementedException();

        public string WebAddress => throw new NotImplementedException();

        public string CompanyName => throw new NotImplementedException();

        public string CompanySiteLink => throw new NotImplementedException();

        public string AppName => throw new NotImplementedException();

        public string AppId => throw new NotImplementedException();

        public string APIToken => throw new NotImplementedException();

        public string AppDescription => throw new NotImplementedException();

        public string TermsAndConditionsLink => throw new NotImplementedException();

        public string PrivacyStatementLink => throw new NotImplementedException();

        public string ClientType => throw new NotImplementedException();

        public string AppLogo => throw new NotImplementedException();

        public string CompanyLogo => throw new NotImplementedException();

        public string InstanceId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string InstanceAuthKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string DeviceId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string DeviceRepoId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string DefaultDeviceLabel => throw new NotImplementedException();

        public string DefaultDeviceLabelPlural => throw new NotImplementedException();

        public bool EmitTestingCode => throw new NotImplementedException();

        public VersionInfo Version => throw new NotImplementedException();

        public string AnalyticsKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}