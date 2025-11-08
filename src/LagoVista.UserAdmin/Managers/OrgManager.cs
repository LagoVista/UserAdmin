// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 391a6f986c2dfe5a3e76803e19ee4dd95e68748b0041c6fe2d5ad70b4174d2f4
// IndexVersion: 2
// --- END CODE INDEX META ---
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
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Models.Auth;
using RingCentral;
using System.Security.Cryptography;
using Newtonsoft.Json;
using LagoVista.Core.Models.Geo;
using LagoVista.Core.Authentication.Models;
using NLog.Targets.Wrappers;

namespace LagoVista.UserAdmin.Managers
{
    public class OrgManager : ManagerBase, IOrganizationManager
    {
        #region Fields
        private readonly IOrganizationRepo _organizationRepo;
        private readonly IOrgLocationRepo _locationRepo;
        private readonly IOrgUserRepo _orgUserRepo;
        private readonly ILocationUserRepo _locationUserRepo;
        private readonly ISmsSender _smsSender;
        private readonly IUserRoleManager _userRoleManager;
        private readonly IUserRoleRepo _userRoleRepo;
        private readonly IEmailSender _emailSender;
        private readonly IInviteUserRepo _inviteUserRepo;
        private readonly ILocationRoleRepo _locationRoleRepo;
        private readonly IAppUserRepo _appUserRepo;
        private readonly IAdminLogger _adminLogger;
        private readonly IOrgInitializer _orgInitializer;
        private readonly IOwnedObjectRepo _ownedObjectRepo;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IDefaultRoleList _defaultRoleList;
        private readonly IAuthenticationLogManager _authLogMgr;
        private readonly ICacheProvider _cacheProvider;
        private readonly IBackgroundServiceTaskQueue _taskQueue;
        private readonly ILocationDiagramRepo _diagramRepo;
        private readonly IRoleRepo _roleRepo;
        #endregion

        #region Ctor
        public OrgManager(IOrganizationRepo organizationRepo,
            IOrgLocationRepo locationRepo,
            IOrgUserRepo orgUserRepo,
            IAppUserRepo appUserRepo,
            IInviteUserRepo inviteUserRepo,
            ILocationUserRepo locationUserRepo,
            ILocationRoleRepo locationRoleRepo,
            ISmsSender smsSender,
            IEmailSender emailSender,
            IAppConfig appConfig,
            IDependencyManager depManager,
            ISecurity security,
            IBackgroundServiceTaskQueue taskQueue,
            IOrgInitializer orgInitializer,
            IDefaultRoleList defaultRoleList,            
            IOwnedObjectRepo ownedObjectRepo,
            IUserRoleRepo userRoleRepo,
            IUserRoleManager useRoleManager,
            IAuthenticationLogManager authLogMgr,
            ISubscriptionManager subscriptionManager,
            ILocationDiagramRepo diagramRepo,
            ICacheProvider cacheProvider,
            IRoleRepo roleRepo,
            IAdminLogger logger) : base(logger, appConfig, depManager, security)
        {

            _authLogMgr = authLogMgr ?? throw new ArgumentNullException(nameof(authLogMgr));
            _appUserRepo = appUserRepo;
            _organizationRepo = organizationRepo;
            _orgUserRepo = orgUserRepo;
            _locationRepo = locationRepo;
            _locationUserRepo = locationUserRepo;
            _subscriptionManager = subscriptionManager;
            _userRoleManager = useRoleManager;
            _locationRoleRepo = locationRoleRepo;
            _smsSender = smsSender;
            _emailSender = emailSender;
            _inviteUserRepo = inviteUserRepo;
            _defaultRoleList = defaultRoleList;
            _userRoleRepo = userRoleRepo;
            _adminLogger = logger;
            _orgInitializer = orgInitializer;
            _ownedObjectRepo = ownedObjectRepo;
            _cacheProvider = cacheProvider;
            _diagramRepo = diagramRepo;
            _taskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
            _roleRepo = roleRepo ?? throw new ArgumentNullException(nameof(roleRepo));
            
        }
        #endregion

        #region Organizations
        public async Task<bool> QueryOrgNamespaceInUseAsync(string namespaceText)
        {
            return await _organizationRepo.QueryNamespaceInUseAsync(namespaceText);
        }

        public async Task<InvokeResult<Organization>> CreateNewOrganizationAsync(CreateOrganizationViewModel organizationViewModel, EntityHeader user)
        {
            var result = new InvokeResult<Organization>();

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
            organization.DefaultProjectLead = user;
            organization.DefaultProjectAdminLead = user;
            organization.DefaultContributor = user;
            organization.DefaultQAResource = user;
            organization.Owner = user;
            organization.IsForProductLine = false;

            /* Create the Organization in Storage */
            await _organizationRepo.AddOrganizationAsync(organization);

            await _authLogMgr.AddAsync(AuthLogTypes.CreatingOrg, user.Id, user.Text, organization.Id, organization.Name);

            var currentUser = await _appUserRepo.FindByIdAsync(user.Id);

            var ownerRoleId = _defaultRoleList.GetStandardRoles().Single(rl => rl.Key == DefaultRoleList.OWNER).Id;
            await _userRoleManager.GrantUserRoleAsync(user.Id, ownerRoleId,organization.ToEntityHeader() , user);

            var addUserResult = await AddUserToOrgAsync(currentUser.ToEntityHeader(), organization.ToEntityHeader(), currentUser.ToEntityHeader(), true, true);
            if (!addUserResult.Successful)
            {
                return InvokeResult<Organization>.FromInvokeResult(addUserResult);
            }

            if (currentUser.Organizations == null) currentUser.Organizations = new List<EntityHeader>();

            /* add the organization ot the newly created user */
            currentUser.Organizations.Add(organization.ToEntityHeader());

            //In this case we are creating a new org for first time through, make sure they have all the correct privelages.
            if (currentUser.CurrentOrganization == null)
            {
                currentUser.IsOrgAdmin = true;
                currentUser.IsAppBuilder = true;
                currentUser.CurrentOrganization = organization.CreateSummary();
                await _authLogMgr.AddAsync(AuthLogTypes.AssignedCurrentOrgToUser, user.Id, user.Text, organization.Id, organization.Name);
            }

            /* Final update of the user */
            await _appUserRepo.UpdateAsync(currentUser);

            /* This isn't working correctly so for now, just do inline, want to background it at some point */
            //await _taskQueue.QueueBackgroundWorkItemAsync(async (token) =>
            //{
            await _authLogMgr.AddAsync(AuthLogTypes.PopulatingNewOrg, user.Id, user.Text, organization.Id, organization.Name);
            await _orgInitializer.Init(organization.ToEntityHeader(), currentUser.ToEntityHeader(), organizationViewModel.CreateGettingStartedData);
            await _authLogMgr.AddAsync(AuthLogTypes.PopulatedNewOrg, user.Id, user.Text, organization.Id, organization.Name);
            //});

            await LogEntityActionAsync(organization.Id, typeof(Organization).Name, "Created Organization", organization.ToEntityHeader(), user);

            await _authLogMgr.AddAsync(AuthLogTypes.CreatedOrg, user.Id, user.Text, organization.Id, organization.Name);

            return InvokeResult<Organization>.Create(organization);
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

            newOrg.IsForProductLine = false;

            await _authLogMgr.AddAsync(AuthLogTypes.ManualOrgCreate, user.Id, user.Text, newOrg.Id, newOrg.Name, newOrg.Namespace);

            await AuthorizeAsync(newOrg, AuthorizeResult.AuthorizeActions.Create, user, userOrg);
            await _organizationRepo.AddOrganizationAsync(newOrg);

            await _orgInitializer.Init(newOrg.ToEntityHeader(), user, true);

            return InvokeResult.Success;
        }

        /// <summary>
        /// Loads a user changes and will save a new version of the user.
        /// </summary>
        /// <param name="newOrgId"></param>
        /// <param name="org"></param>
        /// <param name="user"></param>
        /// <returns></returns>
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
            appUser.CurrentOrganization = newOrg.CreateSummary();
            appUser.IsOrgAdmin = await _orgUserRepo.IsUserOrgAdminAsync(newOrgId, user.Id);
            appUser.IsAppBuilder = await _orgUserRepo.IsAppBuilderAsync(newOrgId, user.Id);

            var orgRoles = await _userRoleRepo.GetRolesForUserAsync(appUser.Id, newOrgId);
            appUser.CurrentOrganizationRoles = new List<EntityHeader>();
            foreach (var orgRole in orgRoles)
            {
                appUser.CurrentOrganizationRoles.Add(orgRole.ToEntityHeader());
            }

            await AuthorizeAsync(appUser, AuthorizeResult.AuthorizeActions.Update, user, org, "switchOrgs");
            await _appUserRepo.UpdateAsync(appUser);
            await _authLogMgr.AddAsync(AuthLogTypes.ChangeOrg, user.Id, user.Text, newOrg.Id, newOrg.Name, extras: $"old orgid: {org.Id}, new orgid: {org.Text}");

            return InvokeResult<AppUser>.Create(appUser);
        }

        /// <summary>
        /// Changes the organization of a user that is already loaded from the database.
        /// </summary>
        /// <param name="newOrgId"></param>
        /// <param name="org"></param>
        /// <param name="appUser"></param>
        /// <returns></returns>
        public async Task<InvokeResult<AppUser>> ChangeOrgsAsync(string newOrgId, EntityHeader org, AppUser appUser)
        {
            if (newOrgId == org.Id)
            {
                return InvokeResult<AppUser>.FromErrors(UserAdminErrorCodes.AuthAlreadyInOrg.ToErrorMessage());
            }

            var hasAccess = await _orgUserRepo.QueryOrgHasUserAsync(newOrgId, appUser.Id);
            if (!hasAccess)
            {
                return InvokeResult<AppUser>.FromErrors(UserAdminErrorCodes.AuthOrgNotAuthorized.ToErrorMessage());
            }

            var newOrg = await _organizationRepo.GetOrganizationAsync(newOrgId);
            appUser.CurrentOrganization = newOrg.CreateSummary();
            appUser.IsOrgAdmin = await _orgUserRepo.IsUserOrgAdminAsync(newOrgId, appUser.Id);
            appUser.IsAppBuilder = await _orgUserRepo.IsAppBuilderAsync(newOrgId, appUser.Id);
            appUser.CurrentOrganizationRoles = new List<EntityHeader>();

            var orgRoles = await _userRoleRepo.GetRolesForUserAsync(appUser.Id, newOrgId);
            appUser.CurrentOrganizationRoles = new List<EntityHeader>();
            foreach (var orgRole in orgRoles)
            {
                appUser.CurrentOrganizationRoles.Add(orgRole.ToEntityHeader());
            }

            await AuthorizeAsync(appUser, AuthorizeResult.AuthorizeActions.Update, appUser.ToEntityHeader(), org, "switchOrgs");
            await _appUserRepo.UpdateAsync(appUser);
            await _authLogMgr.AddAsync(AuthLogTypes.ChangeOrg, appUser.Id, appUser.Name, newOrg.Id, newOrg.Name, extras: $"old orgid: {org.Id}, new orgid: {org.Text}");

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

            var existingOrg = await _organizationRepo.GetOrganizationAsync(org.Id);
            if (existingOrg.IsForProductLine != org.IsForProductLine)
                throw new UnauthorizedAccessException("Attempt to set IsForProductLine in update method, should do in SetIsForProductLine method.");


            await AuthorizeAsync(org, AuthorizeResult.AuthorizeActions.Update, user, userOrg);
            await _organizationRepo.UpdateOrganizationAsync(org);


            return InvokeResult.Success;
        }

        public async Task<InvokeResult<List<GeoLocation>>> GetBoundingBoxForLocationAsync(string orgLocation, EntityHeader org, EntityHeader user)
        {
            var location = await _locationRepo.GetLocationAsync(orgLocation);
            await AuthorizeAsync(location, AuthorizeResult.AuthorizeActions.Update, user, org);

            if(location.DiagramReferences.Any())
            {
                var diagramRef = location.DiagramReferences.First();
                var diagarm = await _diagramRepo.GetLocationDiagramAsync(diagramRef.LocationDiagram.Id);
                var layer = diagarm.Layers.Single(lyr => lyr.Id == diagramRef.LocationDiagramLayer.Id);
                var shape = layer.Shapes.FirstOrDefault(shp => shp.Id == diagramRef.LocationDiagramShape.Id);
                if (shape.GeoPoints.Any())
                    return InvokeResult<List<GeoLocation>>.Create(shape.GeoPoints);

                return InvokeResult<List<GeoLocation>>.FromError("Shape exists but does not have any geo points.");
            }
            else
            {
                return InvokeResult<List<GeoLocation>>.FromError("Location does not have any location diagrams.");
            }
        }

        public async Task<InvokeResult> UpdateLocationAsync(OrgLocation location, EntityHeader org, EntityHeader user)
        {
            ValidationCheck(location, Core.Validation.Actions.Update);

            await AuthorizeAsync(location, AuthorizeResult.AuthorizeActions.Update, user, org);
            await _locationRepo.UpdateLocationAsync(location);

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
            if(EntityHeader.IsNullOrEmpty(org.Owner))
            {
                org.Owner = user;
            }

            await AuthorizeAsync(org, AuthorizeResult.AuthorizeActions.Read, user, userOrg);
            return org;
        }
        #endregion

        #region Invite User
        public async Task<InvokeResult<AcceptInviteResponse>> AcceptInvitationAsync(string inviteId, AppUser acceptedUser)
        {
            var invite = await _inviteUserRepo.GetInvitationAsync(inviteId);

            if (invite == null || !invite.IsActive())
            {
                var reason = invite == null ? "Could not load invite" : $"Status: {invite.Status}";
                await _authLogMgr.AddAsync(AuthLogTypes.AcceptInviteFailed, acceptedUser, inviteId: inviteId, extras: $"Accept not valid to be accepted, Status: {reason}.");
                var acceptResult = InvokeResult<AcceptInviteResponse>.FromErrors(UserAdminErrorCodes.AuthInviteNotActive.ToErrorMessage());
                acceptResult.RedirectURL = $"{CommonLinks.InviteAcceptedFailed}?err={acceptResult.ErrorMessage}";
                return acceptResult;
            }

            var invitingUser = EntityHeader.Create(invite.InvitedById, invite.InvitedByName);
            var orgHeader = EntityHeader.Create(invite.OrganizationId, invite.OrganizationName);

            var result = await AddUserToOrgAsync(acceptedUser.ToEntityHeader(), orgHeader, invitingUser);
            if (!result.Successful)
            {
                await _authLogMgr.AddAsync(AuthLogTypes.AcceptInviteFailed, acceptedUser, inviteId: inviteId, extras: result.ErrorMessage);
                var acceptResult = InvokeResult<AcceptInviteResponse>.FromInvokeResult(result);
                acceptResult.RedirectURL = $"{CommonLinks.InviteAcceptedFailed}?err={result.ErrorMessage}";
                return acceptResult;
            }

            invite.Accepted = true;
            invite.Status = Invitation.StatusTypes.Accepted;
            invite.DateAccepted = DateTime.UtcNow.ToJSONString();
            await _inviteUserRepo.UpdateInvitationAsync(invite);

            await LogEntityActionAsync(acceptedUser.Id, typeof(AppUser).Name, "Accepted Invitation to: " + invite.OrganizationName, orgHeader, acceptedUser.ToEntityHeader());
            await LogEntityActionAsync(invite.InvitedById, typeof(AppUser).Name, "Accepted Invitation to: " + invite.OrganizationName, orgHeader, acceptedUser.ToEntityHeader());
            await LogEntityActionAsync(invite.OrganizationId, typeof(AppUser).Name, $"User {acceptedUser.FirstName} {acceptedUser.LastName} accepted invitation to organization", orgHeader, acceptedUser.ToEntityHeader());

            acceptedUser.CurrentOrganization = (await _organizationRepo.GetOrganizationAsync(invite.OrganizationId)).CreateSummary();
            var existing = acceptedUser.Organizations.FirstOrDefault(org => org.Id == invite.OrganizationId);
            if(existing != null)
                acceptedUser.Organizations.Remove(existing);

            acceptedUser.Organizations.Add(orgHeader);

            await _appUserRepo.UpdateAsync(acceptedUser);

            var msg = $"Congratulations! You have accepted the invitation from {invite.InvitedByName} to the {invite.OrganizationName} organization. ";

            await _authLogMgr.AddAsync(AuthLogTypes.AcceptedInvite, acceptedUser.ToEntityHeader(), acceptedUser.CurrentOrganization.ToEntityHeader(), inviteId: inviteId);

            var response = new AcceptInviteResponse()
            {
                RedirectPage = $"{CommonLinks.InviteAccepted}?inviteid={inviteId}&emailconfirmed={acceptedUser.EmailConfirmed.ToString().ToLower()}",
                ResponseMessage = msg
            };

            if (!String.IsNullOrEmpty(invite.EndUserAppOrg) && !String.IsNullOrEmpty(invite.EndUserAppOrgId))
                response.EndUserAppOrg = EntityHeader.Create(invite.EndUserAppOrgId, invite.EndUserAppOrg);

            if (!String.IsNullOrEmpty(invite.Customer) && !String.IsNullOrEmpty(invite.CustomerId))
                response.Customer = EntityHeader.Create(invite.CustomerId, invite.Customer);

            if (!String.IsNullOrEmpty(invite.CustomerContact) && !String.IsNullOrEmpty(invite.CustomerContactId))
                response.CustomerContact = EntityHeader.Create(invite.CustomerContactId, invite.CustomerContact);

            return InvokeResult<AcceptInviteResponse>.Create(response); 
        }

        public async Task<InvokeResult<AcceptInviteResponse>> AcceptInvitationAsync(string inviteId, string userId)
        {
            var appUser = await _appUserRepo.FindByIdAsync(userId);
            return await AcceptInvitationAsync(inviteId, appUser);
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

            await _authLogMgr.AddAsync(AuthLogTypes.ResendOrgInvitation, userId: user.Id, userName: user.Text, extras: $"Re-Send invite to org {invite.OrganizationName} to {invite.Email} by {user.Text}");

            await AuthorizeAsync(user, org, "ResendInvite", invite);

            var result = await SendInvitationAsync(invite, org, user);
            if (result.Successful)
            {
                await _authLogMgr.AddAsync(AuthLogTypes.ResendOrgInvitationSuccess, userId: user.Id, userName: user.Text, extras: $"Re-Send invite to org {invite.OrganizationName} to {invite.Email} by {user.Text}");

                invite.DateSent = DateTime.Now.ToJSONString();
                invite.Status = Invitation.StatusTypes.Resent;
                await _inviteUserRepo.UpdateInvitationAsync(invite);
            }
            else
            {
                await _authLogMgr.AddAsync(AuthLogTypes.ResendOrgInvitationFailed, userId: user.Id, userName: user.Text, extras: $"Re-Send invite to org {invite.OrganizationName} to {invite.Email} by {user.Text}");
            }

            return result;
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

        //In some cases, this will be called from API, we don't want to return API as part of the link.
        private String GetWebURI()
        {
            var environment = AppConfig.WebAddress;
            if (AppConfig.WebAddress.ToLower().Contains("api"))
            {
                switch (AppConfig.Environment)
                {
                    case Environments.Development: environment = "https://dev.nuviot.com"; break;
                    case Environments.Testing: environment = "https://test.nuviot.com"; break;
                    case Environments.Beta: environment = "https://qa.nuviot.com"; break;
                    case Environments.Staging: environment = "https://stage.nuviot.com"; break;
                    case Environments.Production: environment = "https://www.nuviot.com"; break;
                }
            }

            return environment;
        }

        private async Task<InvokeResult> SendInvitationAsync(Invitation inviteModel, EntityHeader org, EntityHeader user)
        {
            await _authLogMgr.AddAsync(AuthLogTypes.SendingOrgInvitation, userId: user.Id, userName: user.Text, orgId: inviteModel.OrganizationId, orgName: org.Text, extras: $"Sending invite to org {org.Text} to {inviteModel.Email} by {user.Text}");

            var subject = UserAdminResources.Invite_Greeting_Subject.Replace(Tokens.APP_NAME, AppConfig.AppName).Replace(Tokens.ORG_NAME, org.Text);
            var message = UserAdminResources.InviteUser_Greeting_Message.Replace(Tokens.USERS_FULL_NAME, user.Text).Replace(Tokens.ORG_NAME, org.Text).Replace(Tokens.APP_NAME, AppConfig.AppName);
            message += $"<br /><br />{inviteModel.Message}<br /><br />";
            var acceptLink = $"{GetWebURI()}/api/auth/invite/accept/{inviteModel.RowKey}";
            var mobileAcceptLink = $"nuviot://acceptinvite?inviteId={inviteModel.RowKey}";

            message += UserAdminResources.InviteUser_ClickHere.Replace("[ACCEPT_LINK]", acceptLink).Replace("[MOBILE_ACCEPT_LINK]", mobileAcceptLink);

            var result = await _emailSender.SendAsync(inviteModel.Email, subject, message, org, user);
            if (result.Successful)
                await _authLogMgr.AddAsync(AuthLogTypes.SendOrgInvitationSuccess, userId: user.Id, userName: user.Text, orgId: inviteModel.OrganizationId, orgName: org.Text, extras: $"Sent invite to org {org.Text} to {inviteModel.Email} by {user.Text}");
            else
                await _authLogMgr.AddAsync(AuthLogTypes.SendEmailConfirmFailed, userId: user.Id, userName: user.Text, orgId: inviteModel.OrganizationId, orgName: org.Text, errors: result.ErrorMessage, extras: $"Sent invite to org {org.Text} to {inviteModel.Email} by {user.Text}");

            return result;
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


            var emailRegEx = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,12})+)$");
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
            if (existingInvite != null && existingInvite.Status != Invitation.StatusTypes.Accepted)
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

            var appUser = await _appUserRepo.FindByIdAsync(user.Id);

            var inviteModel = new Invitation()
            {
                RowKey = Guid.NewGuid().ToId(),
                PartitionKey = org.Id,
                OrganizationId = org.Id,
                OrganizationName = org.Text,
                InvitedById = user.Id,
                InvitedByName = user.Text,
                InvitedByEmail = appUser.Email,
                Message = inviteViewModel.Message,
                Name = inviteViewModel.Name,
                Email = inviteViewModel.Email,
                DateSent = DateTime.Now.ToJSONString(),
                Status = Invitation.StatusTypes.New,
                EndUserAppOrg = inviteViewModel.EndUserAppOrg?.Text,
                EndUserAppOrgId = inviteViewModel.EndUserAppOrg?.Id,
                Customer = inviteViewModel.Customer?.Text,
                CustomerId = inviteViewModel.Customer?.Id,
                CustomerContact = inviteViewModel.CustomerContact?.Text,
                CustomerContactId = inviteViewModel.CustomerContact?.Id,
            };

            await AuthorizeAsync(user, org, "InviteUser", inviteModel.RowKey);
            inviteModel.OrganizationName = organization.Name;

            await _inviteUserRepo.InsertInvitationAsync(inviteModel);

            inviteModel = await _inviteUserRepo.GetInvitationAsync(inviteModel.RowKey);
            await SendInvitationAsync(inviteModel, org, user);
            inviteModel.DateSent = DateTime.Now.ToJSONString();
            inviteModel.Status = Invitation.StatusTypes.Sent;
            await _inviteUserRepo.UpdateInvitationAsync(inviteModel);
            return InvokeResult<Invitation>.Create(inviteModel);
        }
        #endregion

        #region Organization User Methods
        public async Task<InvokeResult> AddUserToOrgAsync(EntityHeader userToAdd, EntityHeader org, EntityHeader addedBy, bool isOrgAdmin = false, bool isAppBuilder = false)
        {
            var timeStamp = DateTime.UtcNow.ToJSONString();

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
                ProfileImageUrl = appUser.ProfileImage.ImageUrl,
            };

            user.CreatedBy = appUser.Name;
            user.CreatedById = appUser.Id;
            user.CreationDate = timeStamp;
            user.LastUpdatedBy = appUser.Name;
            user.LastUpdatedById = appUser.Id;
            user.LastUpdatedDate = timeStamp;

            await AuthorizeOrgAccessAsync(addedBy, org, typeof(OrgUser), Actions.Create, user);

            await _orgUserRepo.AddOrgUserAsync(user);

            await _authLogMgr.AddAsync(AuthLogTypes.AddUserToOrg, userToAdd.Id, userToAdd.Text, org.Id, org.Text, extras: $"added by id: {addedBy.Id}, name: {addedBy.Text}");

            if (null == appUser.CurrentOrganization)
            {
                var orgDetail = await _organizationRepo.GetOrganizationAsync(org.Id);
                appUser.CurrentOrganization = orgDetail.CreateSummary();
                appUser.LastUpdatedBy = addedBy;
                appUser.LastUpdatedDate = timeStamp;
                appUser.AddChange(nameof(AppUser.CurrentOrganization), "none", appUser.CurrentOrganization.Text);
                await _appUserRepo.UpdateAsync(appUser);
            }

            return result;
        }

        public async Task SetDefaultOrguserRoleAsync(string orgId, string userId, string roleId, EntityHeader userOrg, EntityHeader setByUser)
        {
            await AuthorizeOrgAccessAsync(setByUser, userOrg, typeof(OrgUser), Actions.Update, new SecurityHelper() { OrgId = orgId, UserId = userId });

            
            var role = _defaultRoleList.GetStandardRoles().SingleOrDefault(rl=>rl.Id == roleId);
            if(role == null)
            {
               role = await _roleRepo.GetRoleAsync(roleId);
                if (!role.IsPublic && role.OwnerOrganization.Id != orgId)
                    throw new NotAuthorizedException($"Attempt to assign a default role {roleId} to user {userId}, however role is not available to org: {userOrg.Text}/{userOrg.Id}");
            }

            var orgUser = await _orgUserRepo.GetOrgUserAsync(orgId, userId);

            orgUser.DefaultRole = role.Name;
            orgUser.DefaultRoleId = role.Id;

            orgUser.LastUpdatedBy = setByUser.Text;
            orgUser.LastUpdatedById = setByUser.Id;
            orgUser.LastUpdatedDate = DateTime.UtcNow.ToJSONString();

            await _orgUserRepo.UpdateOrgUserAsync(orgUser);
            await _authLogMgr.AddAsync(AuthLogTypes.AddUserToOrg, orgUser.UserId, orgUser.UserName, userOrg.Id, userOrg.Text, extras: $"set default role {role.Name} by id: {setByUser.Id}, name: {setByUser.Text}");
        }

        public async Task ClearDefaultOrgUserRoleAsync(string orgId, string userId, EntityHeader userOrg, EntityHeader setByUser)
        {
            await AuthorizeOrgAccessAsync(setByUser, userOrg, typeof(OrgUser), Actions.Update, new SecurityHelper() { OrgId = orgId, UserId = userId });


            var orgUser = await _orgUserRepo.GetOrgUserAsync(orgId, userId);

            if (String.IsNullOrEmpty(orgUser.DefaultRoleId))
                return;

            orgUser.DefaultRole = null;
            orgUser.DefaultRoleId = null;

            orgUser.LastUpdatedBy = setByUser.Text;
            orgUser.LastUpdatedById = setByUser.Id;
            orgUser.LastUpdatedDate = DateTime.UtcNow.ToJSONString();

            await _orgUserRepo.UpdateOrgUserAsync(orgUser);
            await _authLogMgr.AddAsync(AuthLogTypes.AddUserToOrg, orgUser.UserId, orgUser.UserName, userOrg.Id, userOrg.Text, extras: $"removed default role by id: {setByUser.Id}, name: {setByUser.Text}");
        }

        public async Task<InvokeResult> AddUserToOrgAsync(string orgId, string userId, EntityHeader userOrg, EntityHeader addedBy)
        {
            var timeStamp = DateTime.UtcNow.ToJSONString();

            await AuthorizeOrgAccessAsync(addedBy, userOrg, typeof(OrgUser), Actions.Create, new SecurityHelper() { OrgId = orgId, UserId = userId });

            var appUser = await _appUserRepo.FindByIdAsync(userId);
            if (appUser == null)
            {
                return InvokeResult.FromError($"Could not find user with user id [{userId}] when attempting to add user to the org [{orgId}]");
            }

            var org = await _organizationRepo.GetOrganizationAsync(orgId);
            if (org == null)
            {
                return InvokeResult.FromError($"Could not find org with org id [{orgId}] when attempting to add a user with id [{userId}] to this org.");
            }

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
                ProfileImageUrl = appUser.ProfileImage.ImageUrl,
            };

            user.CreatedBy = addedBy.Text;
            user.CreatedById = addedBy.Id;
            user.CreationDate = timeStamp;
            user.LastUpdatedBy = addedBy.Text;
            user.LastUpdatedById = addedBy.Id;
            user.LastUpdatedDate = timeStamp;

            await _orgUserRepo.AddOrgUserAsync(user);
            await _authLogMgr.AddAsync(AuthLogTypes.AddUserToOrg, appUser.Id, appUser.Name, org.Id, org.Name, extras: $"added by id: {addedBy.Id}, name: {addedBy.Text}");

            if (null == appUser.CurrentOrganization)
            {
                var orgDetail = await _organizationRepo.GetOrganizationAsync(org.Id);
                appUser.CurrentOrganization = orgDetail.CreateSummary();
                appUser.LastUpdatedBy = addedBy;
                appUser.LastUpdatedDate = timeStamp;
                appUser.AddChange(nameof(AppUser.CurrentOrganization), "none", appUser.CurrentOrganization.Text);
                await _appUserRepo.UpdateAsync(appUser);
            }


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

                var appUser = await _appUserRepo.FindByIdAsync(userId);
                appUser.IsOrgAdmin = true;
                appUser.IsAppBuilder = true;
                appUser.IsAccountDisabled = false;
                await _appUserRepo.UpdateAsync(appUser);

                await LogEntityActionAsync(userId, typeof(AppUser).Name, "SetAsOrgAdmin", org, user);

                await _authLogMgr.AddAsync(AuthLogTypes.SetAsOrgAdmin, appUser.Id, appUser.Name, org.Id, org.Text, extras: $"set by id: {user.Id}, name: {user.Text}");

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

                var appUser = await _appUserRepo.FindByIdAsync(userId);
                appUser.IsOrgAdmin = false;
                appUser.IsAccountDisabled = false;
                await _appUserRepo.UpdateAsync(appUser);

                await LogEntityActionAsync(userId, typeof(AppUser).Name, "ClearAsOrgAdmin", org, user);
                await _authLogMgr.AddAsync(AuthLogTypes.ClearOrgAdmin, appUser.Id, appUser.Name, org.Id, org.Text, extras: $"cleared by id: {user.Id}, name: {user.Text}");

                return InvokeResult.Success;
            }
            else
            {
                return InvokeResult.FromErrors(Resources.UserAdminErrorCodes.AuthNotOrgAdmin.ToErrorMessage());
            }
        }

        public async Task<InvokeResult<string>> GetLandingPageForOrgAsync(string orgId)
        {
            var landingPage = await _organizationRepo.GetHomePageForOrgAsync(orgId);
            return InvokeResult<string>.Create(landingPage);
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

                var appUser = await _appUserRepo.FindByIdAsync(userId);
                appUser.IsAppBuilder = true;
                appUser.IsAccountDisabled = false;
                await _appUserRepo.UpdateAsync(appUser);

                await LogEntityActionAsync(userId, typeof(AppUser).Name, "SetAppBuilder", org, user);

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

                var appUser = await _appUserRepo.FindByIdAsync(userId);
                appUser.IsOrgAdmin = false;
                appUser.IsAppBuilder = false;
                appUser.IsAccountDisabled = false;
                await _appUserRepo.UpdateAsync(appUser);


                await LogEntityActionAsync(userId, typeof(AppUser).Name, "ClearAppBuilder", org, user);

                return InvokeResult.Success;
            }
            else
            {
                return InvokeResult.FromErrors(Resources.UserAdminErrorCodes.AuthNotOrgAdmin.ToErrorMessage());
            }
        }


        public async Task<IEnumerable<UserInfoSummary>> GetUsersForOrganizationsAsync(string orgId, bool useCache, EntityHeader org, EntityHeader user)
        {
            var appUser = await _appUserRepo.FindByIdAsync(user.Id);

            var orgUsers = await _orgUserRepo.GetUsersForOrgAsync(orgId);
            if(!orgUsers.Any(usr=>usr.UserId == user.Id))
            {
                throw new NotAuthorizedException("You do not have access to this org..");
            }

            if (orgUsers.Any())
            {
                return (await _appUserRepo.GetUserSummaryForListAsync(orgUsers, useCache)).OrderBy(usr => usr.Name);
            }
            else
            {
                return new List<UserInfoSummary>();
            }
        }

        public async Task<IEnumerable<UserInfoSummary>> GetActiveUsersForOrganizationsAsync(string orgId, bool useCache, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(OrgUser), Actions.Read, new SecurityHelper() { OrgId = orgId });
            var orgUsers = await _orgUserRepo.GetUsersForOrgAsync(orgId);
            return (await _appUserRepo.GetUserSummaryForListAsync(orgUsers, useCache)).Where(usr => !usr.IsAccountDisabled && !usr.IsRuntimeUser && !usr.IsUserDevice).OrderBy(usr => usr.Name);
        }

        public async Task<IEnumerable<OrgUser>> GetOrganizationsForUserAsync(string userId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(OrgUser), Actions.Read, new SecurityHelper { UserId = userId });
            var orgs = await _orgUserRepo.GetOrgsForUserAsync(userId);
            return orgs.OrderBy(userOrg => userOrg.OrganizationName);
        }

        public async Task<InvokeResult> RemoveUserFromOrganizationAsync(string orgId, string userId, EntityHeader org, EntityHeader user)
        {
            if (org.Id != orgId)
                return InvokeResult.FromError("Org Mismatch, can only remove users from current organization.");

            await AuthorizeOrgAccessAsync(user, org, typeof(OrgUser), Actions.Delete, new SecurityHelper { OrgId = orgId, UserId = userId });

            var appUser = await _appUserRepo.FindByIdAsync(userId);
            var existingOrg = appUser.Organizations.FirstOrDefault(org => org.Id == orgId);
            if(existingOrg != null)
            {
                appUser.Organizations.Remove(existingOrg);
                await _appUserRepo.UpdateAsync(appUser);
            }
            
            await _orgUserRepo.RemoveUserFromOrgAsync(orgId, userId, user);
            await _authLogMgr.AddAsync(AuthLogTypes.RemoveUserFromOrg, appUser.ToEntityHeader(), org, $"Remove user [{appUser.Name}] from the [{org.Text}] by user [{user.Text}]");

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

        public async Task<ListResponse<OrgLocationSummary>> GetLocationsForOrganizationsAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(OrgLocation), Actions.Read, new SecurityHelper { OrgId = org.Id });
            return await _locationRepo.GetOrganizationLocationAsync(org.Id, listRequest);
        }

        public async Task<ListResponse<OrgLocationSummary>> GetLocationsForCustomerAsync(ListRequest listRequest, string customerId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(OrgLocation), Actions.Read, new SecurityHelper { OrgId = org.Id });
            return await _locationRepo.GetOrganizationLocationsForCustomerAsync(org.Id, customerId, listRequest);
        }

        public async Task<OrgLocation> GetOrgLocationAsync(string id, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(OrgLocation), Actions.Read, new SecurityHelper { OrgId = org.Id });

            return await _locationRepo.GetLocationAsync(id);
        }

        public async Task<InvokeResult> DeleteOrgLocationAsync(string id, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(OrgLocation), Actions.Delete, new SecurityHelper { OrgId = org.Id });
            await _locationRepo.DeleteOrgLocationAsync(id);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> AddLocationAsync(OrgLocation location, EntityHeader org, EntityHeader user)
        {
            location.OwnerOrganization = org;
            location.Organization = org;

            SetCreatedBy(location, user);

            ValidationCheck(location, Core.Validation.Actions.Create);

            await AuthorizeAsync(location, AuthorizeResult.AuthorizeActions.Create, user, org);

            await _locationRepo.AddLocationAsync(location);

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

        public async Task<bool> HasBillingRecords(string orgId, EntityHeader org, EntityHeader user)
        {
            var appUser = await _appUserRepo.FindByIdAsync(user.Id);

            if (!appUser.IsSystemAdmin) // Eventually if we need to delete all the data && ((org.Id != orgId) || (org.Id == orgId && !appUser.IsOrgAdmin)))
            {
                //throw new NotAuthorizedException("Must be system admin or belong to the org and be an org admin for the org that is to be deleted, neither of these are the case.");
                throw new NotAuthorizedException("Must be a system admin to check for billing records.");
            }

            return await _organizationRepo.HasBillingRecords(orgId);
        }

        public async Task<InvokeResult> DeleteOrgAsync(string orgId, EntityHeader org, EntityHeader user)
        {
            var appUser = await _appUserRepo.FindByIdAsync(user.Id);
            var fullOrg = await _organizationRepo.GetOrganizationAsync(orgId);

            await AuthorizeAsync(user, org, "DeleteOrganization", fullOrg);

            if (!appUser.IsSystemAdmin) // Eventually if we need to delete all the data && ((org.Id != orgId) || (org.Id == orgId && !appUser.IsOrgAdmin)))
            {
                //throw new NotAuthorizedException("Must be system admin or belong to the org and be an org admin for the org that is to be deleted, neither of these are the case.");
                throw new NotAuthorizedException("Must be a system admin to remove an organization.");
            }

            var hasBillingEvents = await _organizationRepo.HasBillingRecords(orgId);
            if (hasBillingEvents)
            {
                return InvokeResult.FromError("Organization has billing events, can not remove.");
            }

            // 1) Remove any users from the organization 
            var users = await _orgUserRepo.GetUsersForOrgAsync(orgId);
            foreach (var orgUser in users)
            {
                await _orgUserRepo.RemoveUserFromOrgAsync(orgId, orgUser.UserId, user);
                _adminLogger.AddCustomEvent(LogLevel.Message, "UserAdmin_DeleteOrgAsync_RemoveUserFromOrg", $"{user.Text} removed the user {orgUser.UserName} from {orgUser.OrganizationName}");
            }

            _adminLogger.AddCustomEvent(LogLevel.Message, "UserAdmin_DeleteOrgAsync", $"{user.Text} delete the user {fullOrg.Name} from system");

            // 2) Remove subscriptions for the organization.
            var removeSubscriptionsResult = await _subscriptionManager.DeleteSubscriptionsForOrgAsync(orgId, org, user);
            if (!removeSubscriptionsResult.Successful)
            {
                return removeSubscriptionsResult;
            }
        
            // 3) Delete the organization.
            await _organizationRepo.DeleteOrgAsync(orgId);

            // 4) go through any users, if they do not belong to any organizations, remove them.
            foreach (var orgUser in users)
            {
                var orgUsers = await _orgUserRepo.GetOrgsForUserAsync(orgUser.UserId);
                if (!orgUsers.Any())
                {
                    _adminLogger.AddCustomEvent(LogLevel.Message, "UserAdmin_DeleteOrgAsync_DeleteUser", $"{user.Text} delete the user {orgUser.UserName} - {orgUser.Email} from system");
                    await _appUserRepo.DeleteAsync(orgUser.UserId);
                }
                else
                {
                    var existingUserNames = String.Join(",", orgUsers.Select(usrOrg => usrOrg.OrganizationName));
                    _adminLogger.AddCustomEvent(LogLevel.Message, "UserAdmin_DeleteOrgAsync_DidNotDeleteUser", $"{user.Text} did not delete the user {orgUser.UserName} - {orgUser.Email}, user still belongs to orgs [{existingUserNames}].");
                }
            }

            return InvokeResult.Success;
        }

        public async Task<ListResponse<OrganizationSummary>> GetAllOrgsAsync(EntityHeader org, EntityHeader user, ListRequest listRequest)
        {
            var appUser = await _appUserRepo.FindByIdAsync(user.Id);

            if (!appUser.IsSystemAdmin) // Eventually if we need to delete all the data && ((org.Id != orgId) || (org.Id == orgId && !appUser.IsOrgAdmin)))
            {
                //throw new NotAuthorizedException("Must be system admin or belong to the org and be an org admin for the org that is to be deleted, neither of these are the case.");
                throw new NotAuthorizedException("Must be a system admin to get all organizations.");
            }

            return await _organizationRepo.GetAllOrgsAsync(listRequest);
        }

        public async Task<ListResponse<OrganizationSummary>> SearchAllOrgsAsync(string searchOrgName, EntityHeader user, ListRequest listRequest)
        {
            var appUser = await _appUserRepo.FindByIdAsync(user.Id);

            if (!appUser.IsSystemAdmin) // Eventually if we need to delete all the data && ((org.Id != orgId) || (org.Id == orgId && !appUser.IsOrgAdmin)))
            {
                //throw new NotAuthorizedException("Must be system admin or belong to the org and be an org admin for the org that is to be deleted, neither of these are the case.");
                throw new NotAuthorizedException("Must be a system admin to get all organizations.");
            }

            return await _organizationRepo.GetAllOrgsAsync(searchOrgName, listRequest);
        }

        public async Task<ListResponse<OwnedObject>> GetOwnedObjectsForOrgAsync(string orgId, ListRequest request, EntityHeader org, EntityHeader user)
        {
            var appUser = await _appUserRepo.FindByIdAsync(user.Id);

            if (!appUser.IsSystemAdmin) // Eventually if we need to delete all the data && ((org.Id != orgId) || (org.Id == orgId && !appUser.IsOrgAdmin)))
            {
                //throw new NotAuthorizedException("Must be system admin or belong to the org and be an org admin for the org that is to be deleted, neither of these are the case.");
                throw new NotAuthorizedException("Must be a system admin to query owned objects for org.");
            }

            var orgValue = await _organizationRepo.GetOrganizationAsync(orgId);
            await _authLogMgr.AddAsync(AuthLogTypes.SysAdminGetOwnedObjects, user, extras: $"Update Org: ${orgValue.Name} - ${orgValue.Id}");
            return await _ownedObjectRepo.GetOwnedObjectsForOrgAsync(orgId, request);
        }

        public async Task<InvokeResult<Organization>> SysAdminGetOrgAsync(string orgId, EntityHeader user)
        {
            var appUser = await _appUserRepo.FindByIdAsync(user.Id);

            if (!appUser.IsSystemAdmin) // Eventually if we need to delete all the data && ((org.Id != orgId) || (org.Id == orgId && !appUser.IsOrgAdmin)))
            {
                //throw new NotAuthorizedException("Must be system admin or belong to the org and be an org admin for the org that is to be deleted, neither of these are the case.");
                throw new NotAuthorizedException("Must be a system admin get other org.");
            }

            var org = await _organizationRepo.GetOrganizationAsync(orgId);
            await _authLogMgr.AddAsync(AuthLogTypes.SysAdminUpdateOrg, user, extras: $"Update Org: ${org.Name} - ${org.Id}");
            return InvokeResult<Organization>.Create(org);
        }

        public async Task<InvokeResult> SysAdminUpdateOrgAsync(Organization org, EntityHeader user)
        {
            var appUser = await _appUserRepo.FindByIdAsync(user.Id);

            if (!appUser.IsSystemAdmin) // Eventually if we need to delete all the data && ((org.Id != orgId) || (org.Id == orgId && !appUser.IsOrgAdmin)))
            {
                //throw new NotAuthorizedException("Must be system admin or belong to the org and be an org admin for the org that is to be deleted, neither of these are the case.");
                throw new NotAuthorizedException("Must be a system admin to update org.");
            }

            if (EntityHeader.IsNullOrEmpty(org.Owner))
                org.Owner = org.CreatedBy;

            await _authLogMgr.AddAsync(AuthLogTypes.SysAdminUpdateOrg, user, extras: $"Update Org: ${org.Name} - ${org.Id}");
            await _organizationRepo.UpdateOrganizationAsync(org);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> ClearOrgUserCache(EntityHeader org, EntityHeader user)
        {
            // mainly used for testing...
            await _orgUserRepo.ClearOrgCacheAsync(org.Id);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> SetIsProductLineOrgAsync(bool isProductLineOrg, EntityHeader updatedByOrg, EntityHeader user)
        {
            var updateByUser = await _appUserRepo.FindByIdAsync(user.Id);
            if (!updateByUser.IsSystemAdmin)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.SetSystemAdminNotAuthorized, user, extras: $"Attempt to set product line org by non sys-admin user id: {user.Id} {user.Text}");
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthNotSysAdmin.ToErrorMessage());
            }

            var org = await _organizationRepo.GetOrganizationAsync(updatedByOrg.Id);
            org.IsForProductLine = true;
            await _organizationRepo.UpdateOrganizationAsync(org);
            return InvokeResult.Success;
        }
        #endregion

        public async Task<string> GetOrgNameAsync(string orgId)
        {
            var org = await _organizationRepo.GetOrganizationAsync(orgId);
            return org.Name;
        }

        public async Task<string> GetOrgNameSpaceAsync(string orgId)
        {
            var org = await _organizationRepo.GetOrganizationAsync(orgId);
            return org.Namespace;
        }

        public Task<string> GetOrgIdForNameSpaceAsync(string orgNameSpace)
        {
            return _organizationRepo.GetOrganizationIdForNamespaceAsync(orgNameSpace);
        }

        public async Task<PublicOrgInformation> GetPublicOrginfoAsync(string orgns)
        {
            _adminLogger.Trace($"[OrgManager__GetPublicOrginfoAsync] GetPublicOrginfoAsync - Org Namespace {orgns}");
            var id = await _organizationRepo.GetOrganizationIdForNamespaceAsync(orgns);
            var org = await _organizationRepo.GetOrganizationAsync(id);
            return org.ToPublicOrgInfo();
        }

        public async Task<EntityHeader> GetOrgEntityHeaderForNameSpaceAsync(string orgNameSpace)
        {
            var orgId = await _organizationRepo.GetOrganizationIdForNamespaceAsync(orgNameSpace);
            var org = await _organizationRepo.GetOrganizationAsync(orgId);
            return org.ToEntityHeader();
        }


        public async Task<InvokeResult<BasicTheme>> GetBasicThemeForOrgAsync(string orgid)
        {
            var json = await _cacheProvider.GetAsync($"basic_theme_org_{orgid}");
            if (string.IsNullOrEmpty(json))
            {
                var org = await _organizationRepo.GetOrganizationAsync(orgid);
                var basicTheme = new BasicTheme()
                {
                    PrimaryTextColor = org.PrimaryTextColor,
                    PrimryBGColor = org.PrimaryBgColor,
                    AccentColor = org.AccentColor
                };

                await _cacheProvider.AddAsync($"basic_theme_org_{orgid}", JsonConvert.SerializeObject(basicTheme));
                return InvokeResult<BasicTheme>.Create(basicTheme);
            }
            else
            {
                var theme = JsonConvert.DeserializeObject<BasicTheme>(json);
                return InvokeResult<BasicTheme>.Create(theme);

            }           
        }
    }

    public class SecurityHelper
    {
        public String OrgId { get; set; }
        public String LocationId { get; set; }
        public String UserId { get; set; }
        public String RoleId { get; set; }
    }
}