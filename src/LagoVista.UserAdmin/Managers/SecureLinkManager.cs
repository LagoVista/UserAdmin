using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class SecureLinkManager : ManagerBase, ISecureLinkManager
    {
        private readonly ISecureLinkRepo _secureLinkRepo;
        private readonly ILinkShortener _linkShortner;
        private readonly IAppConfig _appConfig;
        private readonly ISignInManager _signInManager;
        private readonly IAppUserManager _appUserManager;

        public SecureLinkManager(ISecureLinkRepo secureLinkRepo, IAppUserManager appUserManager, ISignInManager signInManager, ILinkShortener linkShortener, IDependencyManager depManager, ISecurity security, IAdminLogger logger, IAppConfig appConfig) :
            base(logger, appConfig, depManager, security)
        {
            _secureLinkRepo = secureLinkRepo ?? throw new ArgumentNullException(nameof(secureLinkRepo));
            _linkShortner = linkShortener ?? throw new ArgumentNullException(nameof(linkShortener));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(appConfig));
            _appUserManager = appUserManager ?? throw new ArgumentNullException(nameof(appUserManager));
        }

        public async Task<InvokeResult<String>> GenerateSecureLinkAsync(string link, EntityHeader forUser, TimeSpan duration, EntityHeader org, EntityHeader user)
        {
            var record = new SecureLink()
            {
                RowKey = Guid.NewGuid().ToId(),
                PartitionKey = org.Id,
                Expired = false,
                Expires = DateTime.UtcNow.Add(duration).ToJSONString(),
                DestinationLink = link,
                OrgId = org.Id,
                OrgName = org.Text,
                ForUserId = forUser.Id,
                ForUserName = forUser.Text,
                CreatedByUserId = user.Id,
                CreatedByUser = user.Text,
                CreationDate = DateTime.UtcNow.ToJSONString(),
            };

            await _secureLinkRepo.AddSecureLinkAsync(record);
            var secureLink = $"{_appConfig.WebAddress}/api/links/{org.Id}/{record.RowKey}";
            return await _linkShortner.ShortenLinkAsync(secureLink);
        }

        public async Task<InvokeResult<string>> GetSecureLinkAsync(string orgId, string linkId)
        {
           var secureLink = await  _secureLinkRepo.GetSecureLinkAsync(orgId, linkId);

            secureLink.LastAccess = DateTime.UtcNow.ToString();
            secureLink.AccessCount++;
            await _secureLinkRepo.UpdateSecureLinkAsync(secureLink);

            var org = EntityHeader.Create(secureLink.OrgId, secureLink.OrgName);
            var user = EntityHeader.Create(secureLink.CreatedByUserId, secureLink.CreatedByUser);

            var appUser = await  _appUserManager.GetUserByIdAsync(secureLink.ForUserId, org, user);

            await _signInManager.SignInAsync(appUser);

            return InvokeResult<string>.Create(secureLink.DestinationLink);
        }
    }
}
