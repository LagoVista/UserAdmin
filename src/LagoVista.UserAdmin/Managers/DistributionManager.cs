using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Orgs;
using System;
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

        public DistributionManager(IEmailSender emailSender, ISmsSender smsSender, IDistributionListRepo distroListRepo, 
            IAppUserRepo appUserRepo,  ILogger logger, IAppConfig appConfig, IDependencyManager dependencyManager, ISecurity security) :
            base(logger, appConfig, dependencyManager, security)
        {
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
            _distroListRepo = distroListRepo ?? throw new ArgumentNullException(nameof(distroListRepo));
            _appuserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
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

        public async Task<ListResponse<DistroListSummary>> GetListsForOrgAsync(EntityHeader org, EntityHeader user, ListRequest listRequest)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(DistroList), Actions.Read);
            return await  _distroListRepo.GetDistroListsForOrgAsync(org.Id, listRequest);
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
                await _emailSender.SendAsync(appUserDetail.Email, subject, message);
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
    }
}
