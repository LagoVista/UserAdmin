// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7a14d62c94ddfece173896668ead983abae1af854b721db60427c49e4f3705b9
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using RingCentral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LagoVista.Core.Models.AuthorizeResult;

namespace LagoVista.UserAdmin.Managers
{
    public class DistributionManager : ManagerBase, IDistributionManager
    {
        private readonly IAppUserRepo _appuserRepo;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly IDistributionListRepo _distroListRepo;
        private readonly ILinkShortener _linkShortner;
        private readonly IAppConfig _appConfig;
        private readonly ILogger _logger;

        public DistributionManager(IEmailSender emailSender, ISmsSender smsSender, IDistributionListRepo distroListRepo, ILinkShortener linkShortner,
            IAppUserRepo appUserRepo, ILogger logger, IAppConfig appConfig, IDependencyManager dependencyManager, ISecurity security) :
            base(logger, appConfig, dependencyManager, security)
        {
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
            _distroListRepo = distroListRepo ?? throw new ArgumentNullException(nameof(distroListRepo));
            _appuserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            _linkShortner = linkShortner ?? throw new ArgumentNullException(nameof(linkShortner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<InvokeResult> AddListAsync(DistroList list, EntityHeader org, EntityHeader user)
        {
            ValidationCheck(list, Core.Validation.Actions.Create);
            await AuthorizeAsync(list, AuthorizeResult.AuthorizeActions.Create, user, org);
            await _distroListRepo.AddDistroListAsync(list);
            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUse(string id, EntityHeader org, EntityHeader user)
        {
            var distroList = await _distroListRepo.GetDistroListAsync(id);
            await AuthorizeAsync(distroList, AuthorizeResult.AuthorizeActions.Read, user, org);
            return await CheckForDepenenciesAsync(distroList);
        }

        public async Task<InvokeResult> DeleteListAsync(string id, EntityHeader org, EntityHeader user)
        {
            var distroList = await _distroListRepo.GetDistroListAsync(id);
            await AuthorizeAsync(distroList, AuthorizeActions.Delete, user, org);
            await ConfirmNoDepenenciesAsync(distroList);
            await _distroListRepo.DeleteDistroListAsync(id);
            return InvokeResult.Success;
        }

        public async Task<DistroList> GetListAsync(string id, EntityHeader org, EntityHeader user)
        {
            var distroList = await _distroListRepo.GetDistroListAsync(id);
            await AuthorizeAsync(distroList, AuthorizeActions.Read, user, org);
            return distroList;
        }

        private const string confirmLink = "[ConfirmLink]";

        public async Task<InvokeResult> SendTestAsync(string id, EntityHeader org, EntityHeader user)
        {
            var distroList = await _distroListRepo.GetDistroListAsync(id);

            _logger.Trace($"[DistributionManager__SendTestAsync] - Send Confirmations to Distribution List {distroList.Name}");
            await AuthorizeAsync(distroList, AuthorizeActions.Read, user, org);
            var subject = "Please confirm your email";
            var message = $@"<p>You belong to the {distroList.Name} distribution list.</p>
<p>To ensure you receive notifications as part of this distribution list, please <a href='{confirmLink}'>click here</a> to confirm your email.</p>
<p>Thank you</p>
<p>{user.Text}</p>";

            var smsMessage = $"Please confirm your phone number as part of the {distroList.Name} distribution list. {confirmLink}";

            foreach (var usr in distroList.AppUsers)
            {
                _logger.Trace($"[DistributionManager__SendTestAsync] - Send App User Notification {usr.Text}");

                var appUserDetail = await _appuserRepo.FindByIdAsync(usr.Id);
                var emailLink = await _linkShortner.ShortenLinkAsync($"{_appConfig.WebAddress}/api/distro/{id}/confirm/appuser/{usr.Id}/email");
                await _emailSender.SendAsync(appUserDetail.Email, subject, message.Replace(confirmLink, emailLink.Result), org, user);
                _logger.Trace($"[DistributionManager__SendTestAsync] - Send App User Email {appUserDetail.Email}");

                if (!String.IsNullOrEmpty(appUserDetail.PhoneNumber))
                {
                    var smsLink = await _linkShortner.ShortenLinkAsync($"{_appConfig.WebAddress}/api/distro/{id}/confirm/appuser/{usr.Id}/sms");
                    await _smsSender.SendAsync(appUserDetail.PhoneNumber, smsMessage.Replace(confirmLink, smsLink.Result));
                    _logger.Trace($"[DistributionManager__SendTestAsync] - Send App User Text {appUserDetail.PhoneNumber}");
                }
            }

            foreach (var contact in distroList.ExternalContacts)
            {
                _logger.Trace($"[DistributionManager__SendTestAsync] - Send External Contact Notification {contact.FirstName} {contact.LastName}");

                if (contact.SendEmail)
                {
                    var emailLink = await _linkShortner.ShortenLinkAsync($"{_appConfig.WebAddress}/api/distro/{id}/confirm/external/{contact.Id}/email");
                    await _emailSender.SendAsync(contact.Email, subject, message.Replace(confirmLink, emailLink.Result), org, user);
                    _logger.Trace($"[DistributionManager__SendTestAsync] - Send External Contact Email {contact.Email}");
                }

                if (contact.SendSMS)
                {
                    var smsLink = await _linkShortner.ShortenLinkAsync($"{_appConfig.WebAddress}/api/distro/{id}/confirm/external/{contact.Id}/sms");
                    await _smsSender.SendAsync(contact.Phone, smsMessage.Replace(confirmLink, smsLink.Result));
                    _logger.Trace($"[DistributionManager__SendTestAsync] - Send External Contact Text {contact.Phone}");
                }
            }

            return InvokeResult.Success;
        }

        public async Task<InvokeResult<string>> ConfirmExternalContact(string distroListId, string externalContactId, string contactMethod)
        {
            var distroList = await _distroListRepo.GetDistroListAsync(distroListId);

            var contact = distroList.ExternalContacts.SingleOrDefault(cnt => cnt.Id == externalContactId);
            if (contact != null)
            {
                if (contactMethod == "email")
                    contact.EmailConfirmedTimeStamp = DateTime.UtcNow.ToJSONString();
                else if (contactMethod == "sms")
                    contact.SmsConfirmedTimeStamp = DateTime.UtcNow.ToJSONString();


                await _distroListRepo.UpdateDistroListAsync(distroList);

                return InvokeResult<string>.Create($"<div><h4>Thank you {contact.FirstName} {contact.LastName} for confirming your email address in the {distroList.Name} distribution group.</div>");
            }

            return InvokeResult<string>.Create("Sorry could not find a user with those id.");

        }

        public async Task<InvokeResult<string>> ConfirmAppUserAsync(string distroListId, string appUserId, string contactMethod)
        {
            var distroList = await _distroListRepo.GetDistroListAsync(distroListId);

            var users = distroList.AppUsers.SingleOrDefault(cnt => cnt.Id == appUserId);
            if (users != null)
            {
                var contactDescription = "?";
                if (contactMethod == "email")
                {
                    users.EmailConfirmedTimeStamp = DateTime.UtcNow.ToJSONString();
                    contactDescription = "email address";
                }
                else if (contactMethod == "sms")
                {
                    users.SmsConfirmedTimeStamp = DateTime.UtcNow.ToJSONString();
                    contactDescription = "phone number";
                }

                await _distroListRepo.UpdateDistroListAsync(distroList);

                return InvokeResult<string>.Create($"<div><h4>Thank you {users.Text} for confirming your {contactDescription} address in the {distroList.Name} distribution group.</div>");
            }

            return InvokeResult<string>.Create("Sorry could not find a user with those id.");
        }

        public async Task<ListResponse<DistroListSummary>> GetListsForOrgAsync(EntityHeader org, EntityHeader user, ListRequest listRequest)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(DistroList), Actions.Read);
            return await _distroListRepo.GetDistroListsForOrgAsync(org.Id, listRequest);
        }

        public async Task<ListResponse<DistroListSummary>> GetListsForCustomerAsync(string customerId, EntityHeader org, EntityHeader user, ListRequest listRequest)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(DistroList), Actions.Read);
            return await _distroListRepo.GetDistroListsForCustomerAsync(customerId, org.Id, listRequest);
        }

        public Task<bool> QueryKeyInUseAsync(string key, string orgId)
        {
            return _distroListRepo.QueryKeyInUseAsync(key, orgId);
        }

        public async Task SendEmailNotification(string subject, string message, string distroListId, EntityHeader org, EntityHeader user)
        {
            var distroList = await _distroListRepo.GetDistroListAsync(distroListId);
            await AuthorizeAsync(distroList, AuthorizeResult.AuthorizeActions.Perform, user, org, "sendEmail");

            foreach (var appUser in distroList.AppUsers)
            {
                var appUserDetail = await _appuserRepo.FindByIdAsync(appUser.Id);
                await _emailSender.SendAsync(appUserDetail.Email, subject, message, org, user);
            }
        }

        public async Task SendSmsNotification(string message, string distroListId, EntityHeader org, EntityHeader user)
        {
            var distroList = await _distroListRepo.GetDistroListAsync(distroListId);
            await AuthorizeAsync(distroList, AuthorizeResult.AuthorizeActions.Perform, user, org, "sendEmail");

            foreach (var appUser in distroList.AppUsers)
            {
                var appUserDetail = await _appuserRepo.FindByIdAsync(appUser.Id);
                await _smsSender.SendAsync(appUserDetail.PhoneNumber, message);
            }
        }

        public async Task<InvokeResult> UpdatedListAsync(DistroList list, EntityHeader org, EntityHeader user)
        {
            ValidationCheck(list, Core.Validation.Actions.Update);
            await AuthorizeAsync(list, AuthorizeResult.AuthorizeActions.Update, user, org);
            await _distroListRepo.UpdateDistroListAsync(list);
            return InvokeResult.Success;
        }

        public async Task<List<NotificationContact>> GetAllContactsAsync(EntityHeader distributionList, List<NotificationContact> contacts = null)
        {
            if (contacts == null)
                contacts = new List<NotificationContact>();

            if (!EntityHeader.IsNullOrEmpty(distributionList))
            {
                var distroList = await _distroListRepo.GetDistroListAsync(distributionList.Id);
                contacts.AddRange(distroList.AppUsers.Select(ap => new NotificationContact { Name = ap.Text, AppUserId = ap.Id }));
                contacts.AddRange(distroList.ExternalContacts.Select(ap => new NotificationContact 
                { Name = $"{ap.FirstName} {ap.LastName}", Email = ap.SendEmail ? ap.Email.ToLower() : null, Phone = ap.SendSMS ? ap.Phone : null,}));
            }

            var uniqueAppUsers = contacts.Where(p => !String.IsNullOrEmpty(p.AppUserId)).GroupBy(p => p.AppUserId).Select(p => p.First()).ToList();
            foreach (var usr in uniqueAppUsers)
            {
                var appUser = await _appuserRepo.FindByIdAsync(usr.AppUserId);
                if (appUser != null)
                {
                    foreach (var cnt in contacts)
                    {
                        if (usr.AppUserId == cnt.AppUserId)
                        {
                            cnt.Email = appUser.Email.ToLower();
                            cnt.Phone = appUser.PhoneNumber;
                            cnt.PushNotificationChannels = appUser.PushNotificationChannels;
                        }
                    }
                }
            }

            return contacts
                .GroupBy(p => new { p.Email, p.Phone })
                .Select(g => g.First()).ToList();
        }

        public async Task<List<NotificationContact>> GetAllContactsAsync(string distributionListId, List<NotificationContact> contacts = null)
        {
            if (contacts == null)
                contacts = new List<NotificationContact>();

            if (!String.IsNullOrEmpty(distributionListId))
            {
                var distroList = await _distroListRepo.GetDistroListAsync(distributionListId);
                contacts.AddRange(distroList.AppUsers.Select(ap => new NotificationContact { Name = ap.Text, AppUserId = ap.Id }));
                contacts.AddRange(distroList.ExternalContacts.Select(ap => new NotificationContact
                { Name = $"{ap.FirstName} {ap.LastName}", Email = ap.SendEmail ? ap.Email.ToLower() : null, Phone = ap.SendSMS ? ap.Phone : null, }));
            }

            var uniqueAppUsers = contacts.Where(p => !String.IsNullOrEmpty(p.AppUserId)).GroupBy(p => p.AppUserId).Select(p => p.First()).ToList();
            foreach (var usr in uniqueAppUsers)
            {
                var appUser = await _appuserRepo.FindByIdAsync(usr.AppUserId);
                if (appUser != null)
                {
                    foreach (var cnt in contacts)
                    {
                        if (usr.AppUserId == cnt.AppUserId)
                        {
                            cnt.Email = appUser.Email.ToLower();
                            cnt.Phone = appUser.PhoneNumber;
                            cnt.PushNotificationChannels = appUser.PushNotificationChannels;
                        }
                    }
                }
            }

            return contacts
                .GroupBy(p => new { p.Email, p.Phone })
                .Select(g => g.First()).ToList();
        }

        public async Task<List<NotificationContact>> AddContactAsync(EntityHeader user, List<NotificationContact> contacts = null)
        {
            if (contacts == null)
                contacts = new List<NotificationContact>();

            var appUser = await _appuserRepo.FindByIdAsync(user.Id);
            if (appUser != null)
            {
                var contact = new NotificationContact { Name = appUser.Name, Email = appUser.Email, Phone = appUser.PhoneNumber, AppUserId = appUser.Id, PushNotificationChannels = appUser.PushNotificationChannels };
                contacts.Add(contact);
            }

            return contacts
                .GroupBy(p => new { p.Email, p.Phone })
                .Select(g => g.First()).ToList();
        }


    }
}
