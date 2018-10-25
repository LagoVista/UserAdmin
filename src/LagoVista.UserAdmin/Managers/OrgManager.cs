using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using LagoVista.Core.Exceptions;
using LagoVista.Core;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using System.Linq;
using LagoVista.Core.Managers;
using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Resources;
using LagoVista.UserAdmin.ViewModels.Organization;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Core.Validation;
using LagoVista.Core.Models.UIMetaData;
using System.Text.RegularExpressions;
using LagoVista.UserAdmin.Models.Resources;

namespace LagoVista.UserAdmin.Managers
{
    public class OrgManager : ManagerBase, IOrganizationManager
    {
        #region Fields
        readonly IOrganizationRepo _organizationRepo;
        readonly IOrgLocationRepo _locationRepo;
        readonly IOrgUserRepo _orgUserRepo;
        readonly ILocationUserRepo _locationUserRepo;
        readonly ISmsSender _smsSender;
        readonly IEmailSender _emailSender;
        readonly IInviteUserRepo _inviteUserRepo;
        readonly ILocationRoleRepo _locationRoleRepo;
        readonly IOrganizationRoleRepo _orgRoleRepo;
        readonly IAppUserRepo _appUserRepo;
        readonly IAdminLogger _adminLogger;
        #endregion

        #region Ctor
        public OrgManager(IOrganizationRepo organizationRepo,
            IOrgLocationRepo locationRepo,
            IOrgUserRepo orgUserRepo,
            IAppUserRepo appUserRepo,
            IInviteUserRepo inviteUserRepo,
            ILocationUserRepo locationUserRepo,
            ILocationRoleRepo locationRoleRepo,
            IOrganizationRoleRepo orgRoleRepo,
            ISmsSender smsSender,
            IEmailSender emailSender,
            IAppConfig appConfig,
            IDependencyManager depManager,
            ISecurity security,
            IAdminLogger logger) : base(logger, appConfig, depManager, security)
        {

            _appUserRepo = appUserRepo;
            _organizationRepo = organizationRepo;
            _orgUserRepo = orgUserRepo;
            _locationRepo = locationRepo;
            _locationUserRepo = locationUserRepo;

            _orgRoleRepo = orgRoleRepo;
            _locationRoleRepo = locationRoleRepo;
            _smsSender = smsSender;
            _emailSender = emailSender;
            _inviteUserRepo = inviteUserRepo;
            _adminLogger = logger;
        }
        #endregion

        #region Organizations
        public async Task<bool> QueryOrgNamespaceInUseAsync(string namespaceText)
        {
            return await _organizationRepo.QueryNamespaceInUseAsync(namespaceText);
        }

        public async Task<InvokeResult> CreateNewOrganizationAsync(CreateOrganizationViewModel organizationViewModel, EntityHeader user)
        {
            var result = new InvokeResult();

            ValidationCheck(organizationViewModel, Core.Validation.Actions.Create);

            //HACK: Very, small chance, but it does exist...two entries could be added at exact same time and check would fail...need to make progress, can live with risk for now.
            if (await _organizationRepo.QueryNamespaceInUseAsync(organizationViewModel.Namespace))
            {
                result.Errors.Add(new ErrorMessage(UserAdminResources.Organization_NamespaceInUse.Replace(Tokens.NAMESPACE, organizationViewModel.Namespace)));
                return result;
            }

            var organization = new Organization();
            organization.SetId();
            organization.SetCreationUpdatedFields(user);
            organizationViewModel.MapToOrganization(organization);
            organization.Status = UserAdminResources.Organization_Status_Active;
            organization.TechnicalContact = user;
            organization.AdminContact = user;
            organization.BillingContact = user;

            /* Create the Organization in Storage */
            await _organizationRepo.AddOrganizationAsync(organization);

            var currentUser = await _appUserRepo.FindByIdAsync(user.Id);

            var addUserResult = await AddUserToOrgAsync(currentUser.ToEntityHeader(), organization.ToEntityHeader(), currentUser.ToEntityHeader(), true, true);
            if (!addUserResult.Successful)
            {
                return addUserResult;
            }

            if (EntityHeader.IsNullOrEmpty(currentUser.CurrentOrganization)) currentUser.CurrentOrganization = organization.ToEntityHeader();
            if (currentUser.Organizations == null) currentUser.Organizations = new List<EntityHeader>();

            /* add the organization ot the newly created user */
            currentUser.Organizations.Add(organization.ToEntityHeader());

            /* Final update of the user */
            await _appUserRepo.UpdateAsync(currentUser);

            await LogEntityActionAsync(organization.Id, typeof(Organization).Name, "Created Organization", organization.ToEntityHeader(), user);

            return result;
        }

        public async Task<InvokeResult> CreateOrganizationAsync(Organization newOrg, EntityHeader userOrg, EntityHeader user)
        {
            /* This means that the user is creating the org upon sign up, 
             * just go ahead and assign this org as the owner org. */
            if (userOrg == null)
            {
                newOrg.OwnerOrganization = newOrg.ToEntityHeader();
            }

            ValidationCheck(newOrg, Core.Validation.Actions.Create);

            await AuthorizeAsync(newOrg, AuthorizeResult.AuthorizeActions.Create, user, userOrg);
            await _organizationRepo.AddOrganizationAsync(newOrg);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult<AppUser>> ChangeOrgsAsync(string newOrgId, EntityHeader org, EntityHeader user)
        {
            if (newOrgId == org.Id)
            {
                return InvokeResult<AppUser>.FromErrors(UserAdminErrorCodes.AuthAlreadyInOrg.ToErrorMessage());
            }

            var hasAccess = await _orgUserRepo.QueryOrgHasUserAsync(newOrgId, user.Id);
            if (!hasAccess)
            {
                return InvokeResult<AppUser>.FromErrors(UserAdminErrorCodes.AuthOrgNotAuthorized.ToErrorMessage());
            }

            var newOrg = await _organizationRepo.GetOrganizationAsync(newOrgId);
            var appUser = await _appUserRepo.FindByIdAsync(user.Id);
            appUser.CurrentOrganization = newOrg.ToEntityHeader();
            appUser.IsOrgAdmin = await _orgUserRepo.IsUserOrgAdminAsync(newOrgId, user.Id);
            appUser.IsAppBuilder = await _orgUserRepo.IsAppBuilderAsync(newOrgId, user.Id);
           
            await AuthorizeAsync(appUser, AuthorizeResult.AuthorizeActions.Update, user, org, "switchOrgs");
            await _appUserRepo.UpdateAsync(appUser);

            return InvokeResult<AppUser>.Create(appUser);
        }

        public Task<bool> IsUserOrgAdminAsync(string orgId, string userId)
        {
            return _orgUserRepo.IsUserOrgAdminAsync(orgId, userId);
        }


        public Task<bool> IsUserAppBuildernAsync(string orgId, string userId)
        {
            return _orgUserRepo.IsAppBuilderAsync(orgId, userId);
        }

        public async Task<InvokeResult> UpdateOrganizationAsync(Organization org, EntityHeader userOrg, EntityHeader user)
        {
            ValidationCheck(org, Core.Validation.Actions.Update);

            await AuthorizeAsync(org, AuthorizeResult.AuthorizeActions.Update, user, userOrg);
            await _organizationRepo.UpdateOrganizationAsync(org);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> CreateLocationAsync(OrgLocation location, EntityHeader org, EntityHeader user)
        {
            ValidationCheck(location, Core.Validation.Actions.Create);

            await AuthorizeAsync(location, AuthorizeResult.AuthorizeActions.Create, user, org);
            await _locationRepo.AddLocationAsync(location);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateLocationAsync(OrgLocation location, EntityHeader org, EntityHeader user)
        {
            ValidationCheck(location, Core.Validation.Actions.Update);

            await AuthorizeAsync(location, AuthorizeResult.AuthorizeActions.Update, user, org);
            await _locationRepo.UpdateLocationAsync(location);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateOrganizationAsync(UpdateOrganizationViewModel orgViewModel, EntityHeader userOrg, EntityHeader user)
        {
            ValidationCheck(orgViewModel, Core.Validation.Actions.Update);

            var org = await _organizationRepo.GetOrganizationAsync(orgViewModel.OrganziationId);
            await AuthorizeAsync(org, AuthorizeResult.AuthorizeActions.Update, user, userOrg);

            org.SetLastUpdatedFields(user);
            ConcurrencyCheck(org, orgViewModel.LastUpdatedDate);

            await _organizationRepo.UpdateOrganizationAsync(org);

            return InvokeResult.Success;
        }

        public async Task<UpdateOrganizationViewModel> GetUpdateOrganizationViewModel(string orgId, EntityHeader userOrg, EntityHeader user)
        {
            /* Only gets a view model with the content of the organization, doesn't do any updating */
            var org = await _organizationRepo.GetOrganizationAsync(orgId);
            await AuthorizeAsync(org, AuthorizeResult.AuthorizeActions.Update, user, userOrg);
            return UpdateOrganizationViewModel.CreateFromOrg(org);
        }

        public async Task<Organization> GetOrganizationAsync(string orgId, EntityHeader userOrg, EntityHeader user)
        {
            ValidateAuthParams(userOrg, user);

            var org = await _organizationRepo.GetOrganizationAsync(orgId);
            await AuthorizeAsync(org, AuthorizeResult.AuthorizeActions.Read, user, userOrg);
            return org;
        }
        #endregion

        #region Invite User
        public async Task<InvokeResult> AcceptInvitationAsync(string inviteId, string acceptedUserId)
        {
            var invite = await _inviteUserRepo.GetInvitationAsync(inviteId);
            if (!invite.IsActive())
            {
                return InvokeResult.FromErrors(Resources.UserAdminErrorCodes.AuthInviteNotActive.ToErrorMessage());
            }

            invite.Accepted = true;
            invite.Status = Invitation.StatusTypes.Accepted;
            invite.DateAccepted = DateTime.UtcNow.ToJSONString();
            await _inviteUserRepo.UpdateInvitationAsync(invite);

            var acceptedUser = await _appUserRepo.FindByIdAsync(acceptedUserId);
            var invitingUser = EntityHeader.Create(invite.InvitedById, invite.InvitedByName);
            var orgHeader = EntityHeader.Create(invite.OrganizationId, invite.OrganizationName);

            await LogEntityActionAsync(acceptedUserId, typeof(AppUser).Name, "Accepted Invitation to: " + invite.OrganizationName, orgHeader, acceptedUser.ToEntityHeader());
            await LogEntityActionAsync(invite.InvitedById, typeof(AppUser).Name, "Accepted Invitation to: " + invite.OrganizationName, orgHeader, acceptedUser.ToEntityHeader());
            await LogEntityActionAsync(invite.OrganizationId, typeof(AppUser).Name, $"User {acceptedUser.FirstName} {acceptedUser.LastName} accepted invitation to organization", orgHeader, acceptedUser.ToEntityHeader());

            var result = await AddUserToOrgAsync(acceptedUser.ToEntityHeader(), orgHeader, invitingUser);
            if (!result.Successful)
            {
                return result;
            }

            if (acceptedUser.CurrentOrganization == null || acceptedUser.CurrentOrganization.IsEmpty())
            {
                acceptedUser.CurrentOrganization = orgHeader;                
            }

            acceptedUser.Organizations.Add(orgHeader);

            await _appUserRepo.UpdateAsync(acceptedUser);

            return InvokeResult.Success;
        }

        public Task<InvokeResult> AcceptInvitationAsync(AcceptInviteViewModel acceptInviteViewModel, String acceptedUserId)
        {
            return AcceptInvitationAsync(acceptInviteViewModel.InvitedById, acceptedUserId);
        }

        public async Task<InvokeResult> AcceptInvitationAsync(string inviteId, EntityHeader orgHeader, EntityHeader user)
        {
            var invite = await _inviteUserRepo.GetInvitationAsync(inviteId);
            if (invite == null)
            {
                return "Could Not Find Invitation".ToInvokeResult();
            }

            if (invite.Accepted)
            {
                return "This invitation has already been accepted".ToInvokeResult();
            }

            invite.Accepted = true;
            invite.Status = Invitation.StatusTypes.Accepted;
            invite.DateAccepted = DateTime.UtcNow.ToJSONString();

            var newOrgHeader = new EntityHeader()
            {
                Id = invite.OrganizationId,
                Text = invite.OrganizationName
            };

            await _inviteUserRepo.UpdateInvitationAsync(invite);

            var invitingUser = EntityHeader.Create(invite.InvitedById, invite.InvitedByName);

            return await AddUserToOrgAsync(user, newOrgHeader, invitingUser);
        }

        public Task<Invitation> GetInvitationAsync(String invigtationId)
        {
            return _inviteUserRepo.GetInvitationAsync(invigtationId);
        }

        public async Task<ListResponse<Invitation>> GetInvitationsAsync(ListRequest request, EntityHeader org, EntityHeader user, Invitation.StatusTypes? byStatus = null)
        {
            ValidateAuthParams(org, user);

            await AuthorizeOrgAccessAsync(user, org, typeof(Invitation), Actions.Read);
            return await _inviteUserRepo.GetInvitationsForOrgAsync(org.Id, request, byStatus);
        }


        public async Task<ListResponse<Invitation>> GetActiveInvitationsForOrgAsync(ListRequest request, EntityHeader org, EntityHeader user)
        {
            ValidateAuthParams(org, user);

            await AuthorizeOrgAccessAsync(user, org, typeof(Invitation), Actions.Read);
            return await _inviteUserRepo.GetActiveInvitationsForOrgAsync(org.Id, request);
        }

        public async Task<InvokeResult> ResendInvitationAsync(String inviteId, EntityHeader org, EntityHeader user)
        {
            ValidateAuthParams(org, user);

            var invite = await _inviteUserRepo.GetInvitationAsync(inviteId);

            await AuthorizeAsync(user, org, "ResendInvite", invite);

            await SendInvitationAsync(invite, invite.OrganizationName, user);

            invite.DateSent = DateTime.Now.ToJSONString();
            invite.Status = Invitation.StatusTypes.Resent;
            await _inviteUserRepo.UpdateInvitationAsync(invite);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> RevokeInvitationAsync(String inviteId, EntityHeader org, EntityHeader user)
        {
            ValidateAuthParams(org, user);

            var invite = await _inviteUserRepo.GetInvitationAsync(inviteId);
            if (invite != null)
            {

                await AuthorizeAsync(user, org, "revokeInvite", invite.RowKey);
                invite.Status = Invitation.StatusTypes.Revoked;
                await _inviteUserRepo.UpdateInvitationAsync(invite);

                return InvokeResult.Success;
            }
            else
            {
                return InvokeResult.FromErrors(new ErrorMessage("Could Not Find Invite Id to Remove"));
            }
        }

        public async Task DeclineInvitationAsync(String inviteId)
        {
            var invite = await _inviteUserRepo.GetInvitationAsync(inviteId);
            invite.Status = Invitation.StatusTypes.Declined;
            await _inviteUserRepo.UpdateInvitationAsync(invite);
        }

        public async Task<AcceptInviteViewModel> GetInviteViewModelAsync(String inviteId)
        {
            var invite = await _inviteUserRepo.GetInvitationAsync(inviteId);
            return AcceptInviteViewModel.CreateFromInvite(invite);
        }

        public async Task<bool> GetIsInvigationActiveAsync(string inviteId)
        {
            var invite = await _inviteUserRepo.GetInvitationAsync(inviteId);
            if (invite == null)
            {
                return false;
            }

            return invite.Status == Invitation.StatusTypes.Sent || invite.Status == Invitation.StatusTypes.Resent || invite.Status == Invitation.StatusTypes.Replaced;
        }

        private async Task SendInvitationAsync(Invitation inviteModel, string orgName, EntityHeader user)
        {

            var subject = UserAdminResources.Invite_Greeting_Subject.Replace(Tokens.APP_NAME, AppConfig.AppName).Replace(Tokens.ORG_NAME, orgName);
            var message = UserAdminResources.InviteUser_Greeting_Message.Replace(Tokens.USERS_FULL_NAME, user.Text).Replace(Tokens.ORG_NAME, orgName).Replace(Tokens.APP_NAME, AppConfig.AppName);
            message += $"<br /><br />{inviteModel.Message}<br /><br />";
            var acceptLink = $"{AppConfig.WebAddress}/account/acceptinvite/{inviteModel.RowKey}";
            var mobileAcceptLink = $"nuviot://acceptinvite?inviteId={inviteModel.RowKey}";

            message += UserAdminResources.InviteUser_ClickHere.Replace("[ACCEPT_LINK]", acceptLink).Replace("[MOBILE_ACCEPT_LINK]", mobileAcceptLink);

            await _emailSender.SendAsync(inviteModel.Email, subject, message);
        }

        public async Task<InvokeResult<Invitation>> InviteUserAsync(Models.DTOs.InviteUser inviteViewModel, EntityHeader org, EntityHeader user)
        {

            ValidateAuthParams(org, user);


            if (inviteViewModel == null)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "OrgManager_InviteUserAsync", UserAdminErrorCodes.InviteIsNull.Message);
                return InvokeResult<Invitation>.FromErrors(UserAdminErrorCodes.InviteIsNull.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(inviteViewModel.Email))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "OrgManager_InviteUserAsync", UserAdminErrorCodes.InviteEmailIsEmpty.Message);
                return InvokeResult<Invitation>.FromErrors(UserAdminErrorCodes.InviteEmailIsEmpty.ToErrorMessage());
            }


            var emailRegEx = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            if (!emailRegEx.Match(inviteViewModel.Email).Success)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "OrgManager_InviteUserAsync", UserAdminErrorCodes.InviteEmailIsInvalid.Message);
                return InvokeResult<Invitation>.FromErrors(UserAdminErrorCodes.InviteEmailIsInvalid.ToErrorMessage());
            }


            if (String.IsNullOrEmpty(inviteViewModel.Name))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "OrgManager_InviteUserAsync", UserAdminErrorCodes.InviteNameIsEmpty.Message);
                return InvokeResult<Invitation>.FromErrors(UserAdminErrorCodes.InviteNameIsEmpty.ToErrorMessage());
            }

            if (await _orgUserRepo.QueryOrgHasUserByEmailAsync(org.Id, inviteViewModel.Email))
            {
                var existingUser = await _appUserRepo.FindByEmailAsync(inviteViewModel.Email);
                if (existingUser != null)
                {
                    var msg = UserAdminResources.InviteUser_AlreadyPartOfOrg.Replace(Tokens.USERS_FULL_NAME, existingUser.Name).Replace(Tokens.EMAIL_ADDR, inviteViewModel.Email);
                    return InvokeResult<Invitation>.FromErrors(new ErrorMessage(msg));
                }
                else
                {
                    _adminLogger.AddError("OrgManager_InviteUserAsync", "User Found in Org Unit XRef Table Storage, but not in User, bad data", new KeyValuePair<string, string>("OrgId", org.Id), new KeyValuePair<string, string>("Email", inviteViewModel.Email));
                }
            }

            var existingInvite = await _inviteUserRepo.GetInviteByOrgIdAndEmailAsync(org.Id, inviteViewModel.Email);
            if (existingInvite != null)
            {
                existingInvite.Status = Invitation.StatusTypes.Replaced;
                await _inviteUserRepo.UpdateInvitationAsync(existingInvite);
            }

            Organization organization;

            organization = await _organizationRepo.GetOrganizationAsync(org.Id);
            if (organization == null)
            {
                /* Quick and Dirty Error Checking, should Never Happen */
                return InvokeResult<Invitation>.FromError("Could not Load Org");
            }

            var inviteModel = new Invitation()
            {
                RowKey = Guid.NewGuid().ToId(),
                PartitionKey = org.Id,
                OrganizationId = org.Id,
                OrganizationName = org.Text,
                InvitedById = user.Id,
                InvitedByName = user.Text,
                Message = inviteViewModel.Message,
                Name = inviteViewModel.Name,
                Email = inviteViewModel.Email,
                DateSent = DateTime.Now.ToJSONString(),
                Status = Invitation.StatusTypes.New,
            };

            await AuthorizeAsync(user, org, "InviteUser", inviteModel.RowKey);
            inviteModel.OrganizationName = organization.Name;

            await _inviteUserRepo.InsertInvitationAsync(inviteModel);

            inviteModel = await _inviteUserRepo.GetInvitationAsync(inviteModel.RowKey);
            await SendInvitationAsync(inviteModel, organization.Name, user);
            inviteModel.DateSent = DateTime.Now.ToJSONString();
            inviteModel.Status = Invitation.StatusTypes.Sent;
            await _inviteUserRepo.UpdateInvitationAsync(inviteModel);
            return InvokeResult<Invitation>.Create(inviteModel);
        }
        #endregion

        #region Organization User Methods
        public async Task<InvokeResult> AddUserToOrgAsync(EntityHeader userToAdd, EntityHeader org, EntityHeader addedBy, bool isOrgAdmin = false, bool isAppBuilder = false)
        {
            await AuthorizeOrgAccessAsync(addedBy, org, typeof(OrgUser), Actions.Create, new SecurityHelper() { OrgId = org.Id, UserId = userToAdd.Id });

            var result = InvokeResult.Success;
            var appUser = await _appUserRepo.FindByIdAsync(userToAdd.Id);

            if (await _orgUserRepo.QueryOrgHasUserAsync(org.Id, userToAdd.Id))
            {
                var couldntAddResult = new InvokeResult();
                couldntAddResult.Errors.Add(new ErrorMessage(UserAdminResources.OrganizationUser_UserExists.Replace(Tokens.USERS_FULL_NAME, appUser.Name).Replace(Tokens.ORG_NAME, org.Text)));
                return couldntAddResult;
            }

            var user = new OrgUser(org.Id, userToAdd.Id)
            {
                Email = appUser.Email,
                OrganizationName = org.Text,
                UserName = appUser.Name,
                IsOrgAdmin = isOrgAdmin,
                IsAppBuilder = isAppBuilder,
                ProfileImageUrl = appUser.ProfileImageUrl.ImageUrl,
            };

            user.CreatedBy = appUser.Name;
            user.CreatedById = appUser.Id;
            user.CreationDate = DateTime.UtcNow.ToJSONString();
            user.LastUpdatedBy = appUser.Name;
            user.LastUpdatedById = appUser.Id;
            user.LastUpdatedDate = user.CreationDate;

            await AuthorizeOrgAccessAsync(addedBy, org, typeof(OrgUser), Actions.Create, user);

            await _orgUserRepo.AddOrgUserAsync(user);

            return result;
        }

        public async Task<InvokeResult> AddUserToOrgAsync(string orgId, string userId, EntityHeader userOrg, EntityHeader addedBy)
        {
            await AuthorizeOrgAccessAsync(addedBy, userOrg, typeof(OrgUser), Actions.Create, new SecurityHelper() { OrgId = orgId, UserId = userId });

            var appUser = await _appUserRepo.FindByIdAsync(userId);
            var org = await _organizationRepo.GetOrganizationAsync(orgId);

            await AuthorizeOrgAccessAsync(addedBy, org.ToEntityHeader(), typeof(OrgUser));

            if (await _orgUserRepo.QueryOrgHasUserAsync(orgId, userId))
            {
                var result = new InvokeResult();
                result.Errors.Add(new ErrorMessage(UserAdminResources.OrganizationUser_UserExists.Replace(Tokens.USERS_FULL_NAME, appUser.Name).Replace(Tokens.ORG_NAME, org.Name)));
                return result;
            }

            var user = new OrgUser(org.Id, userId)
            {
                Email = appUser.Email,
                OrganizationName = org.Name,
                IsOrgAdmin = false,
                IsAppBuilder = false,
                UserName = appUser.Name,
                ProfileImageUrl = appUser.ProfileImageUrl.ImageUrl,
            };

            user.CreatedBy = addedBy.Text;
            user.CreatedById = addedBy.Text;
            user.CreationDate = DateTime.UtcNow.ToJSONString();
            user.LastUpdatedBy = addedBy.Text;
            user.LastUpdatedById = addedBy.Id;
            user.LastUpdatedDate = user.CreationDate;

            await _orgUserRepo.AddOrgUserAsync(user);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> SetOrgAdminAsync(string userId, EntityHeader org, EntityHeader user)
        {
            var isUpdateUserOrgAdmin = await _orgUserRepo.IsUserOrgAdminAsync(org.Id, user.Id);
            if (isUpdateUserOrgAdmin)
            {
                var orgUser = await _orgUserRepo.GetOrgUserAsync(org.Id, userId);
                orgUser.IsOrgAdmin = true;
                orgUser.IsAppBuilder = true;
                orgUser.LastUpdatedBy = user.Text;
                orgUser.LastUpdatedById = user.Id;
                orgUser.LastUpdatedDate = DateTime.UtcNow.ToJSONString();
                await _orgUserRepo.UpdateOrgUserAsync(orgUser);

                await LogEntityActionAsync(userId, typeof(AppUser).Name, "SetAsOrgAdmin", org, user);

                return InvokeResult.Success;
            }
            else
            {
                return InvokeResult.FromErrors(Resources.UserAdminErrorCodes.AuthNotOrgAdmin.ToErrorMessage());
            }
        }

        public async Task<InvokeResult> ClearOrgAdminAsync(string userId, EntityHeader org, EntityHeader user)
        {
            var isUpdateUserOrgAdmin = await _orgUserRepo.IsUserOrgAdminAsync(org.Id, user.Id);
            if (isUpdateUserOrgAdmin)
            {
                if (user.Id == userId)
                {
                    return InvokeResult.FromErrors(Resources.UserAdminErrorCodes.AuthCantRemoveSelfFromOrgAdmin.ToErrorMessage());
                }

                var orgUser = await _orgUserRepo.GetOrgUserAsync(org.Id, userId);
                orgUser.IsOrgAdmin = false;
                orgUser.LastUpdatedBy = user.Text;
                orgUser.LastUpdatedById = user.Id;
                orgUser.LastUpdatedDate = DateTime.UtcNow.ToJSONString();
                await _orgUserRepo.UpdateOrgUserAsync(orgUser);

                await LogEntityActionAsync(userId, typeof(AppUser).Name, "ClearAsOrgAdmin", org, user);

                return InvokeResult.Success;
            }
            else
            {
                return InvokeResult.FromErrors(Resources.UserAdminErrorCodes.AuthNotOrgAdmin.ToErrorMessage());
            }
        }

        public async Task<InvokeResult> SetAppBuilderAsync(string userId, EntityHeader org, EntityHeader user)
        {
            var isUpdateUserOrgAdmin = await _orgUserRepo.IsUserOrgAdminAsync(org.Id, user.Id);
            if (isUpdateUserOrgAdmin)
            {
                if (user.Id == userId)
                {
                    return InvokeResult.FromErrors(Resources.UserAdminErrorCodes.AuthCantRemoveSelfFromOrgAdmin.ToErrorMessage());
                }

                var orgUser = await _orgUserRepo.GetOrgUserAsync(org.Id, userId);
                orgUser.IsAppBuilder = true;
                orgUser.IsOrgAdmin = false;
                orgUser.LastUpdatedBy = user.Text;
                orgUser.LastUpdatedById = user.Id;
                orgUser.LastUpdatedDate = DateTime.UtcNow.ToJSONString();
                await _orgUserRepo.UpdateOrgUserAsync(orgUser);

                await LogEntityActionAsync(userId, typeof(AppUser).Name, "SetAsOrgAdmin", org, user);

                return InvokeResult.Success;
            }
            else
            {
                return InvokeResult.FromErrors(Resources.UserAdminErrorCodes.AuthNotOrgAdmin.ToErrorMessage());
            }
        }

        public async Task<InvokeResult> ClearAppBuilderAsync(string userId, EntityHeader org, EntityHeader user)
        {
            var isUpdateUserOrgAdmin = await _orgUserRepo.IsUserOrgAdminAsync(org.Id, user.Id);
            if (isUpdateUserOrgAdmin)
            {
                if (user.Id == userId)
                {
                    return InvokeResult.FromErrors(Resources.UserAdminErrorCodes.AuthCantRemoveSelfFromOrgAdmin.ToErrorMessage());
                }

                var orgUser = await _orgUserRepo.GetOrgUserAsync(org.Id, userId);
                orgUser.IsOrgAdmin = false;
                orgUser.IsAppBuilder = false;
                orgUser.LastUpdatedBy = user.Text;
                orgUser.LastUpdatedById = user.Id;
                orgUser.LastUpdatedDate = DateTime.UtcNow.ToJSONString();
                await _orgUserRepo.UpdateOrgUserAsync(orgUser);

                await LogEntityActionAsync(userId, typeof(AppUser).Name, "ClearAsOrgAdmin", org, user);

                return InvokeResult.Success;
            }
            else
            {
                return InvokeResult.FromErrors(Resources.UserAdminErrorCodes.AuthNotOrgAdmin.ToErrorMessage());
            }
        }


        public async Task<IEnumerable<UserInfoSummary>> GetUsersForOrganizationsAsync(string orgId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(OrgUser), Actions.Read, new SecurityHelper() { OrgId = orgId });
            var orgUsers = await _orgUserRepo.GetUsersForOrgAsync(orgId);
            var userIds = (from orgUser in orgUsers select orgUser.UserId).ToList();
            return await _appUserRepo.GetUserSummaryForListAsync(orgUsers);
        }

        public async Task<IEnumerable<OrgUser>> GetOrganizationsForUserAsync(string userId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(OrgUser), Actions.Read, new SecurityHelper { UserId = userId });
            return await _orgUserRepo.GetOrgsForUserAsync(userId);
        }

        public async Task<InvokeResult> RemoveUserFromOrganizationAsync(string orgId, string userId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(OrgUser), Actions.Delete, new SecurityHelper { OrgId = orgId, UserId = userId });
            await _orgUserRepo.RemoveUserFromOrgAsync(orgId, userId, user);
            return InvokeResult.Success;

        }

        public async Task<bool> QueryOrganizationHasUserAsync(string orgId, string userId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(OrgUser), Actions.Read, new SecurityHelper { OrgId = orgId, UserId = userId });
            return await _orgUserRepo.QueryOrgHasUserAsync(orgId, userId);
        }
        #endregion

        #region Organization Location
        public async Task<bool> QueryLocationNamespaceInUseAsync(string orgId, string namespaceText)
        {
            return await _locationRepo.QueryNamespaceInUseAsync(orgId, namespaceText);
        }

        public async Task<IEnumerable<OrgLocation>> GetLocationsForOrganizationsAsync(string orgId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(OrgLocation), Actions.Read, new SecurityHelper { OrgId = orgId });
            return (await _locationRepo.GetOrganizationLocationAsync(orgId)).ToList();
        }

        public async Task<InvokeResult> AddLocationAsync(CreateLocationViewModel newLocation, EntityHeader org, EntityHeader user)
        {
            var location = new OrgLocation();
            newLocation.MapToOrganizationLocation(location);

            location.IsPublic = false;
            location.Organization = org;
            location.OwnerOrganization = org;
            if (EntityHeader.IsNullOrEmpty(location.AdminContact)) location.AdminContact = user;
            if (EntityHeader.IsNullOrEmpty(location.TechnicalContact)) location.TechnicalContact = user;

            SetCreatedBy(location, user);

            ValidationCheck(location, Core.Validation.Actions.Create);

            await AuthorizeAsync(location, AuthorizeResult.AuthorizeActions.Create, user, org);

            await _locationRepo.AddLocationAsync(location);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> AddLocationAsync(OrgLocation location, EntityHeader org, EntityHeader user)
        {
            location.IsPublic = false;
            location.OwnerOrganization = org;
            location.Organization = org;
            if (EntityHeader.IsNullOrEmpty(location.AdminContact)) location.AdminContact = user;
            if (EntityHeader.IsNullOrEmpty(location.TechnicalContact)) location.TechnicalContact = user;

            SetCreatedBy(location, user);

            ValidationCheck(location, Core.Validation.Actions.Create);

            await AuthorizeAsync(location, AuthorizeResult.AuthorizeActions.Create, user, org);

            await _locationRepo.AddLocationAsync(location);

            return InvokeResult.Success;
        }

        public CreateLocationViewModel GetCreateLocationViewModel(EntityHeader org, EntityHeader user)
        {
            return CreateLocationViewModel.CreateNew(org, user);
        }

        public async Task<UpdateLocationViewModel> GetUpdateLocationViewModelAsync(String locationId, EntityHeader org, EntityHeader user)
        {
            var location = await _locationRepo.GetLocationAsync(locationId);
            return UpdateLocationViewModel.CreateForOrganizationLocation(location);
        }

        public async Task<InvokeResult> UpdateLocationAsync(UpdateLocationViewModel location, EntityHeader org, EntityHeader user)
        {
            var locationFromStorage = await _locationRepo.GetLocationAsync(location.LocationId);
            ConcurrencyCheck(locationFromStorage, location.LastUpdatedDate);
            SetLastUpdated(locationFromStorage, user);

            ValidationCheck(location, Core.Validation.Actions.Update);
            await AuthorizeAsync(locationFromStorage, AuthorizeResult.AuthorizeActions.Update, user, org);

            await _locationRepo.UpdateLocationAsync(locationFromStorage);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> AddUserRoleForOrgAsync(EntityHeader org, EntityHeader user, EntityHeader role, EntityHeader userOrg, EntityHeader addedBy)
        {
            await AuthorizeOrgAccessAsync(addedBy, userOrg, typeof(OrganizationUserRole), Actions.Create, new SecurityHelper { UserId = user.Id, OrgId = org.Id, RoleId = role.Id });
            var orgUserRole = new OrganizationUserRole(org, user)
            {
                RoleName = role.Text,
                RoleId = role.Id
            };

            await _orgRoleRepo.AddRoleForUserAsync(orgUserRole);

            return InvokeResult.Success;
        }

        /// <summary>
        /// Returns roles for a user in an org
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="userId"></param>
        /// <param name="org">Permissions</param>
        /// <param name="user">Permissions</param>
        /// <returns></returns>
        public async Task<IEnumerable<OrganizationUserRole>> GetUsersRolesInOrgAsync(string orgId, string userId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(OrganizationUserRole), Actions.Read, new SecurityHelper { OrgId = orgId, UserId = userId });
            return await _orgRoleRepo.GetRolesForUserAsync(userId, orgId);
        }

        /// <summary>
        /// Return Users that have a role in an org
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="roleId"></param>
        /// <param name="org">Permissions</param>
        /// <param name="user">Permissions</param>
        /// <returns></returns>
        public async Task<IEnumerable<OrganizationUserRole>> GetUserWithRoleInOrgAsync(string orgId, string roleId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(OrganizationUserRole), Actions.Read, new SecurityHelper { OrgId = orgId, RoleId = roleId });
            return await _orgRoleRepo.GetUserForRoleAsync(orgId, roleId);
        }

        public async Task<InvokeResult> RevokeRoleForUserInOrgAsync(string orgId, string userId, string roleId, EntityHeader userOrg, EntityHeader revokedBy)
        {
            await AuthorizeOrgAccessAsync(revokedBy, userOrg, typeof(OrganizationUserRole), Actions.Delete, new SecurityHelper { OrgId = orgId, RoleId = roleId, UserId = userId });
            await _orgRoleRepo.RevokeRoleForUserInOrgAsync(orgId, userId, roleId);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> RevokeAllRolesForUserInOrgAsync(string orgId, string userId, EntityHeader org, EntityHeader revokedBy)
        {
            await AuthorizeOrgAccessAsync(revokedBy, org, typeof(OrganizationUserRole), Actions.Delete, new SecurityHelper { OrgId = orgId, UserId = userId });
            await _orgRoleRepo.RevokeAllRolesForUserInOrgAsync(orgId, userId);
            return InvokeResult.Success;
        }
        #endregion

        #region Location User
        public async Task<InvokeResult> AddUserToLocationAsync(String userId, String locationId, EntityHeader org, EntityHeader addedBy)
        {
            await AuthorizeOrgAccessAsync(addedBy, org, typeof(LocationUser), Actions.Create, new SecurityHelper { UserId = userId, LocationId = locationId });

            var appUser = await _appUserRepo.FindByIdAsync(userId);
            var location = await _locationRepo.GetLocationAsync(locationId);

            var locationUser = new LocationUser(location.Organization.Id, locationId, userId)
            {
                UsersName = appUser.Name
            };

            await _locationUserRepo.AddUserToLocationAsync(locationUser);

            return InvokeResult.Success;
        }

        public async Task<IEnumerable<LocationUser>> GetUsersForLocationAsync(string locationId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(LocationUser), Actions.Read, new SecurityHelper { LocationId = locationId });
            return await _locationUserRepo.GetUsersForLocationAsync(locationId);
        }

        public async Task<IEnumerable<LocationUser>> GetLocationsForUserAsync(string userId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(LocationUser), Actions.Read, new SecurityHelper { UserId = userId });
            return await _locationUserRepo.GetLocationsForUserAsync(userId);
        }

        public async Task<InvokeResult> RemoveUserFromLocationAsync(string locationId, string userId, EntityHeader org, EntityHeader removedBy)
        {
            await AuthorizeOrgAccessAsync(removedBy, org, typeof(LocationUser), Actions.Delete, new SecurityHelper { LocationId = locationId, UserId = userId });
            await _locationUserRepo.RemoveUserFromLocationAsync(locationId, userId, removedBy);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> AddUserRoleForLocationAsync(EntityHeader location, EntityHeader user, EntityHeader role, EntityHeader org, EntityHeader addedBy)
        {
            await AuthorizeOrgAccessAsync(addedBy, org, typeof(LocationUserRole), Actions.Read, new SecurityHelper { LocationId = location.Id, UserId = user.Id, RoleId = role.Id });
            var locationUserRole = new LocationUserRole(location, user)
            {
                RoleId = role.Id,
                RoleName = role.Text
            };

            await _locationRoleRepo.AddRoleForUserAsync(locationUserRole);
            return InvokeResult.Success;
        }

        /// <summary>
        /// Return list of roles that a user fills in a location
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="userId"></param>
        /// <param name="org"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<IEnumerable<LocationUserRole>> GetRolesForUserInLocationAsync(string locationId, string userId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(LocationUserRole), Actions.Read, data: new SecurityHelper { LocationId = locationId, UserId = userId });
            return await _locationRoleRepo.GetRolesForUserInLocationAsync(locationId, userId);
        }

        /// <summary>
        /// Return users that fill a specific role in a location.
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="roleId"></param>
        /// <param name="org">permissions</param>
        /// <param name="user">permissions</param>
        /// <returns></returns>
        public async Task<IEnumerable<LocationUserRole>> GetUserWithRoleInLocationAsync(string locationId, string roleId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(LocationUserRole), Actions.Read, new SecurityHelper { LocationId = locationId, RoleId = roleId });
            return await _locationRoleRepo.GetUsersInRoleForLocationAsync(locationId, roleId);
        }

        public async Task<InvokeResult> RevokeRoleForUserInLocationAsync(string locationId, string userId, string roleId, EntityHeader org, EntityHeader revokedBy)
        {
            await AuthorizeOrgAccessAsync(revokedBy, org, typeof(LocationUserRole), action: Actions.Delete, data: new SecurityHelper { LocationId = locationId, UserId = userId, RoleId = roleId });
            await _locationRoleRepo.RevokeRoleForUserInLocationAsync(locationId, userId, roleId);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> RevokeAllRolesForUserInLocationAsync(string locationId, string userId, EntityHeader org, EntityHeader revokedBy)
        {
            await AuthorizeOrgAccessAsync(revokedBy, org, typeof(LocationUserRole), action: Actions.Delete, data: new SecurityHelper { LocationId = locationId, UserId = userId });
            await _locationRoleRepo.RevokeAllRolesForUserInLocationAsync(locationId, userId);
            return InvokeResult.Success;
        }
        #endregion
    }

    public class SecurityHelper
    {
        public String OrgId { get; set; }
        public String LocationId { get; set; }
        public String UserId { get; set; }
        public String RoleId { get; set; }
    }
}