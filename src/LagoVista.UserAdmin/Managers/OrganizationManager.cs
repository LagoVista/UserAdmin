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
using LagoVista.UserAdmin.Interfaces.Repos.Account;
using LagoVista.UserAdmin.Resources;
using LagoVista.UserAdmin.ViewModels.Organization;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Account;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Core.Validation;

namespace LagoVista.UserAdmin.Managers
{
    public class OrganizationManager : ManagerBase, IOrganizationManager
    {
        #region Fields
        readonly IOrganizationRepo _organizationRepo;
        readonly IOrganizationLocationRepo _locationRepo;
        readonly IOrganizationAccountRepo _orgAccountRepo;
        readonly ILocationAccountRepo _locationAccountRepo;
        readonly ISmsSender _smsSender;
        readonly IEmailSender _emailSender;
        readonly IInviteUserRepo _inviteUserRepo;
        readonly ILocationRoleRepo _locationRoleRepo;
        readonly IOrganizationRoleRepo _orgRoleRepo;
        readonly IAppUserRepo _appUserRepo;
        #endregion

        #region Ctor
        public OrganizationManager(IOrganizationRepo organizationRepo,
            IOrganizationLocationRepo locationRepo,
            IOrganizationAccountRepo orgAccountRepo,
            IAppUserRepo appUserRepo,
            IInviteUserRepo inviteUserRepo,
            ILocationAccountRepo locationAccountRepo,
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
            _orgAccountRepo = orgAccountRepo;
            _locationRepo = locationRepo;
            _locationAccountRepo = locationAccountRepo;

            _orgRoleRepo = orgRoleRepo;
            _locationRoleRepo = locationRoleRepo;

            _smsSender = smsSender;
            _emailSender = emailSender;
            _inviteUserRepo = inviteUserRepo;
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

            //HACK: Very, small chance, but it does exist...two entries could be added at exact same time and check would fail...need to make progress, can live with rish for now.
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

            var location = new OrganizationLocation();
            location.SetId();
            organizationViewModel.MapToOrganizationLocation(location);
            location.SetCreationUpdatedFields(user);
            location.AdminContact = user;
            location.TechnicalContact = user;
            location.OwnerOrganization = organization.ToEntityHeader();
            location.Organization = organization.ToEntityHeader();

            organization.PrimaryLocation = location.ToEntityHeader();

            if (organization.Locations == null) organization.Locations = new List<EntityHeader>();
            organization.Locations.Add(location.ToEntityHeader());

            var currentUser = await _appUserRepo.FindByIdAsync(user.Id);
            var locationUser = new LocationAccount(organization.Id, location.Id, user.Id)
            {
                Email = currentUser.Email,
                OrganizationName = organization.Name,
                AccountName = currentUser.Name,
                ProfileImageUrl = currentUser.ProfileImageUrl.ImageUrl,
                LocationName = location.Name
            };
            locationUser.SetCreationUpdatedFields(user);

            /* At this point nothing has been written to storage, would be nice to wrap the following in a transaction...*/
            //TODO: Can we wrap the following in a transaction?

            var addUserResult = await AddAccountToOrgAsync(currentUser.ToEntityHeader(), organization.ToEntityHeader(), currentUser.ToEntityHeader());
            if(!addUserResult.Successful)
            {
                return addUserResult;
            }            

            /* Create the Organization in Storage */
            await _organizationRepo.AddOrganizationAsync(organization);     
            
            /* create the location */
            await _locationRepo.AddLocationAsync(location);

            /* add the account to the location */
            await _locationAccountRepo.AddAccountToLocationAsync(locationUser);

            if (EntityHeader.IsNullOrEmpty(currentUser.CurrentOrganization)) currentUser.CurrentOrganization = organization.ToEntityHeader();
            if (currentUser.Organizations == null) currentUser.Organizations = new List<EntityHeader>();

            /* add the organization ot the newly created user */
            currentUser.Organizations.Add(organization.ToEntityHeader());

            /* Final update of the user */
            await _appUserRepo.UpdateAsync(currentUser);

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

        public async Task<InvokeResult> UpdateOrganizationAsync(Organization org, EntityHeader userOrg, EntityHeader user)
        {
            ValidationCheck(org, Core.Validation.Actions.Update);

            await AuthorizeAsync(org, AuthorizeResult.AuthorizeActions.Update, user, userOrg);
            await _organizationRepo.UpdateOrganizationAsync(org);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> CreateLocationAsync(OrganizationLocation location, EntityHeader org, EntityHeader user)
        {
            ValidationCheck(location, Core.Validation.Actions.Create);

            await AuthorizeAsync(location, AuthorizeResult.AuthorizeActions.Create, user, org);
            await _locationRepo.AddLocationAsync(location);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateLocationAsync(OrganizationLocation location, EntityHeader org, EntityHeader user)
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

        public async Task<UpdateOrganizationViewModel> GetUpdateOrganizationViewModel(string organizationId, EntityHeader userOrg, EntityHeader user)
        {
            /* Only gets a view model with the content of the organization, doesn't do any updating */
            var org = await _organizationRepo.GetOrganizationAsync(organizationId);
            await AuthorizeAsync(org, AuthorizeResult.AuthorizeActions.Update, user, userOrg);
            return UpdateOrganizationViewModel.CreateFromOrg(org);
        }

        public async Task<Organization> GetOrganizationAsync(string organizationId, EntityHeader userOrg, EntityHeader user)
        {
            var org = await _organizationRepo.GetOrganizationAsync(organizationId);
            await AuthorizeAsync(org, AuthorizeResult.AuthorizeActions.Read, user, userOrg);
            return org;
        }
        #endregion

        #region Invite User
        public async Task<InvokeResult> AcceptInvitationAsync(AcceptInviteViewModel acceptInviteViewModel, String acceptedUserId)
        {
            var invite = await _inviteUserRepo.GetInvitationAsync(acceptInviteViewModel.InviteId);
            invite.Accepted = true;
            invite.Status = Invitation.StatusTypes.Accepted;
            invite.DateAccepted = DateTime.Now.ToJSONString();
            await _inviteUserRepo.UpdateInvitationAsync(invite);

            var acceptedUser = await _appUserRepo.FindByIdAsync(acceptedUserId);
            var invitingUser = EntityHeader.Create(invite.InvitedById, invite.InvitedByName);
            var orgHeader = EntityHeader.Create(invite.OrganizationId, invite.OrganizationName);

            var result = await AddAccountToOrgAsync(acceptedUser.ToEntityHeader(), orgHeader, invitingUser);
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

        public async Task<InvokeResult> RevokeInvitationAsync(String inviteId, EntityHeader org, EntityHeader user)
        {
            var invite = await _inviteUserRepo.GetInvitationAsync(inviteId);

            await AuthorizeAsync(user, org, "revokeInvite", invite);
            invite.Status = Invitation.StatusTypes.Revoked;
            await _inviteUserRepo.UpdateInvitationAsync(invite);

            return InvokeResult.Success;
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

        public async Task<InvokeResult<Invitation>> InviteUserAsync(InviteUserViewModel inviteViewModel, EntityHeader orgEntityHeader, EntityHeader userEntityHeader)
        {
            var result = new InvokeResult<Invitation>();

            if (await _orgAccountRepo.QueryOrganizationHasAccountByEmailAsync(orgEntityHeader.Id, inviteViewModel.Email))
            {
                var existingUser = await _appUserRepo.FindByEmailAsync(inviteViewModel.Email);
                var msg = UserAdminResources.InviteUser_AlreadyPartOfOrg.Replace(Tokens.USERS_FULL_NAME, existingUser.Name).Replace(Tokens.EMAIL_ADDR, inviteViewModel.Email);
                result.Errors.Add(new ErrorMessage(msg));
                return result;
            }

            var invite = await _inviteUserRepo.GetInviteByOrgIdAndEmailAsync(orgEntityHeader.Id, inviteViewModel.Email);
            if (invite != null)
            {
                invite.Status = Invitation.StatusTypes.Replaced;
                await _inviteUserRepo.UpdateInvitationAsync(invite);
            }

            var inviteModel = new Invitation()
            {
                RowKey = Guid.NewGuid().ToId(),
                PartitionKey = orgEntityHeader.Id,
                OrganizationId = orgEntityHeader.Id,
                OrganizationName = orgEntityHeader.Text,
                InvitedById = userEntityHeader.Id,
                InvitedByName = userEntityHeader.Text,
                Name = inviteViewModel.Name,
                Email = inviteViewModel.Email,
                DateSent = DateTime.Now.ToJSONString(),
                Status = Invitation.StatusTypes.New,
            };

            await AuthorizeAsync(userEntityHeader, orgEntityHeader, "inviteuser", inviteModel);
            await _inviteUserRepo.InsertInvitationAsync(inviteModel);

            var subject = Resources.UserAdminResources.Invite_Greeting_Subject.Replace(Tokens.APP_NAME, AppConfig.AppName).Replace(Tokens.ORG_NAME, orgEntityHeader.Text);
            var message = Resources.UserAdminResources.InviteUser_Greeting_Message.Replace(Tokens.USERS_FULL_NAME, userEntityHeader.Text).Replace(Tokens.ORG_NAME, orgEntityHeader.Text).Replace(Tokens.APP_NAME, AppConfig.AppName);
            message += $"<br /><br />{inviteViewModel.Message}<br /><br />";
            var acceptLink = $"{AppConfig.WebAddress}/organization/acceptinvite/{inviteModel.RowKey}";

            message += Resources.UserAdminResources.InviteUser_ClickHere.Replace("[ACCEPT_LINK]", acceptLink);

            await _emailSender.SendAsync(inviteModel.Email, subject, message);

            inviteModel = await _inviteUserRepo.GetInvitationAsync(inviteModel.RowKey);
            inviteModel.DateSent = DateTime.Now.ToJSONString();
            inviteModel.Status = Invitation.StatusTypes.Sent;
            await _inviteUserRepo.UpdateInvitationAsync(inviteModel);
            result.Result = inviteModel;

            return result;
        }
        #endregion

        #region Organization Account Methods
        public async Task<InvokeResult> AddAccountToOrgAsync(EntityHeader userToAdd, EntityHeader org, EntityHeader addedBy)
        {
            await AuthorizeOrgAccessAsync(addedBy, org, typeof(OrganizationAccount), Actions.Create, new SecurityHelper() { OrgId = org.Id, UserId = userToAdd.Id });

            var result = InvokeResult.Success;
            var appUser = await _appUserRepo.FindByIdAsync(userToAdd.Id);

            if (await _orgAccountRepo.QueryOrganizationHasUserAsync(org.Id, userToAdd.Id))
            {
                throw new ValidationException(Resources.UserAdminResources.OrganizationAccount_CouldntAdd, false,
                    UserAdminResources.OrganizationAccount_UserExists.Replace(Tokens.USERS_FULL_NAME, appUser.Name).Replace(Tokens.ORG_NAME, org.Text));
            }

            var accountUser = new OrganizationAccount(org.Id, userToAdd.Id)
            {
                Email = appUser.Email,
                OrganizationName = org.Text,
                AccountName = appUser.Name,
                ProfileImageUrl = appUser.ProfileImageUrl.ImageUrl,
            };

            accountUser.CreatedBy = appUser.Name;
            accountUser.CreatedById = appUser.Id;
            accountUser.CreationDate = DateTime.Now.ToJSONString();
            accountUser.LastUpdatedBy = appUser.Name;
            accountUser.LastUpdatedById = appUser.Id;
            accountUser.LastUpdatedDate = accountUser.CreationDate;

            await AuthorizeOrgAccessAsync(addedBy, org, typeof(OrganizationAccount), Actions.Create, accountUser);

            await _orgAccountRepo.AddAccountUserAsync(accountUser);

            return result;
        }

        public async Task<InvokeResult> AddAccountToOrgAsync(string orgId, string userId, EntityHeader userOrg, EntityHeader addedBy)
        {
            await AuthorizeOrgAccessAsync(addedBy, userOrg, typeof(OrganizationAccount), Actions.Create, new SecurityHelper() { OrgId = orgId, UserId = userId });

            var appUser = await _appUserRepo.FindByIdAsync(userId);
            var org = await _organizationRepo.GetOrganizationAsync(orgId);

            await AuthorizeOrgAccessAsync(addedBy, org.ToEntityHeader(), typeof(OrganizationAccount));

            if (await _orgAccountRepo.QueryOrganizationHasUserAsync(orgId, userId))
            {
                var result = new InvokeResult();
                result.Errors.Add(new ErrorMessage(UserAdminResources.OrganizationAccount_UserExists.Replace(Tokens.USERS_FULL_NAME, appUser.Name).Replace(Tokens.ORG_NAME, org.Name)));
                return result;
            }

            var accountUser = new OrganizationAccount(org.Id, userId)
            {
                Email = appUser.Email,
                OrganizationName = org.Name,
                AccountName = appUser.Name,
                ProfileImageUrl = appUser.ProfileImageUrl.ImageUrl,
            };

            accountUser.CreatedBy = addedBy.Text;
            accountUser.CreatedById = addedBy.Text;
            accountUser.CreationDate = DateTime.Now.ToJSONString();
            accountUser.LastUpdatedBy = addedBy.Text;
            accountUser.LastUpdatedById = addedBy.Id;
            accountUser.LastUpdatedDate = accountUser.CreationDate;

            await _orgAccountRepo.AddAccountUserAsync(accountUser);

            return InvokeResult.Success;
        }

        public async Task<IEnumerable<OrganizationAccount>> GetUsersForOrganizationsAsync(string orgId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(OrganizationAccount), Actions.Read, new SecurityHelper() { OrgId = orgId });
            return await _orgAccountRepo.GetAccountsForOrganizationAsync(orgId);
        }

        public async Task<IEnumerable<OrganizationAccount>> GetOrganizationsForAccountAsync(string userId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(OrganizationAccount), Actions.Read, new SecurityHelper { UserId = userId });
            return await _orgAccountRepo.GetOrganizationsForAccountAsync(userId);
        }

        public async Task<InvokeResult> RemoveUserFromOrganizationAsync(string orgId, string userId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(OrganizationAccount), Actions.Delete, new SecurityHelper { OrgId = orgId, UserId = userId });
            await _orgAccountRepo.RemoveAccountFromOrgAsync(orgId, userId, user);
            return InvokeResult.Success;

        }

        public async Task<bool> QueryOrganizationHasAccountAsync(string orgId, string userId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(OrganizationAccount), Actions.Read, new SecurityHelper { OrgId = orgId, UserId = userId });
            return await _orgAccountRepo.QueryOrganizationHasUserAsync(orgId, userId);
        }
        #endregion

        #region Organization Location
        public async Task<bool> QueryLocationNamespaceInUseAsync(string orgId, string namespaceText)
        {
            return await _locationRepo.QueryNamespaceInUseAsync(orgId, namespaceText);
        }

        public async Task<IEnumerable<OrganizationLocation>> GetLocationsForOrganizationsAsync(string orgId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(OrganizationLocation), Actions.Read, new SecurityHelper { OrgId = orgId });
            return (await _locationRepo.GetOrganizationLocationAsync(orgId)).ToList();
        }

        public async Task<InvokeResult> AddLocationAsync(CreateLocationViewModel newLocation, EntityHeader org, EntityHeader user)
        {
            var location = new OrganizationLocation();
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

        public async Task<InvokeResult> AddLocationAsync(OrganizationLocation location, EntityHeader org, EntityHeader user)
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

        public async Task<InvokeResult> AddAccountRoleForOrgAsync(EntityHeader org, EntityHeader user, EntityHeader role, EntityHeader userOrg, EntityHeader addedBy)
        {
            await AuthorizeOrgAccessAsync(addedBy, userOrg, typeof(OrganizationUserRole), Actions.Create, new SecurityHelper { UserId = user.Id, OrgId = org.Id, RoleId = role.Id });
            var orgUserRole = new OrganizationUserRole(org, user)
            {
                RoleName = role.Text,
                RoleId = role.Id
            };

            await _orgRoleRepo.AddRoleForAccountAsync(orgUserRole);

            return InvokeResult.Success;
        }

        /// <summary>
        /// Returns roles for a user in an org
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="accountId"></param>
        /// <param name="org">Permissions</param>
        /// <param name="user">Permissions</param>
        /// <returns></returns>
        public async Task<IEnumerable<OrganizationUserRole>> GetAccountRolesForOrgAsync(string orgId, string userId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(OrganizationUserRole), Actions.Read, new SecurityHelper { OrgId = orgId, UserId = userId });
            return await _orgRoleRepo.GetRolesForAccountAsync(userId, orgId);
        }

        /// <summary>
        /// Return Users that have a role in an org
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="roleId"></param>
        /// <param name="org">Permissions</param>
        /// <param name="user">Permissions</param>
        /// <returns></returns>
        public async Task<IEnumerable<OrganizationUserRole>> GetAccountsForRoleInOrgAsync(string orgId, string roleId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(OrganizationUserRole), Actions.Read, new SecurityHelper { OrgId = orgId, RoleId = roleId });
            return await _orgRoleRepo.GetAccountsForRoleAsync(orgId, roleId);
        }

        public async Task<InvokeResult> RevokeRoleForAccountInOrgAsync(string orgId, string userId, string roleId, EntityHeader userOrg, EntityHeader revokedBy)
        {
            await AuthorizeOrgAccessAsync(revokedBy, userOrg, typeof(OrganizationUserRole), Actions.Delete, new SecurityHelper { OrgId = orgId, RoleId = roleId, UserId = userId });
            await _orgRoleRepo.RevokeRoleForAccountInOrgAsync(orgId, userId, roleId);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> RevokeAllRolesForAccountInOrgAsync(string orgId, string userId, EntityHeader org, EntityHeader revokedBy)
        {
            await AuthorizeOrgAccessAsync(revokedBy, org, typeof(OrganizationUserRole), Actions.Delete, new SecurityHelper { OrgId = orgId, UserId = userId });
            await _orgRoleRepo.RevokeAllRolesForAccountInOrgAsync(orgId, userId);
            return InvokeResult.Success;
        }
        #endregion

        #region Location Account
        public async Task<InvokeResult> AddAccountToLocationAsync(String accountId, String locationId, EntityHeader org, EntityHeader addedBy)
        {
            await AuthorizeOrgAccessAsync(addedBy, org, typeof(LocationAccount), Actions.Create, new SecurityHelper { UserId = accountId, LocationId = locationId });

            var appUser = await _appUserRepo.FindByIdAsync(accountId);
            var location = await _locationRepo.GetLocationAsync(locationId);

            var locationAccount = new LocationAccount(location.Organization.Id, locationId, accountId)
            {
                AccountName = appUser.Name
            };

            await _locationAccountRepo.AddAccountToLocationAsync(locationAccount);

            return InvokeResult.Success;
        }

        public async Task<IEnumerable<LocationAccount>> GetAccountsForLocationAsync(string locationId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(LocationAccount), Actions.Read, new SecurityHelper { LocationId = locationId });
            return await _locationAccountRepo.GetAccountsForLocationAsync(locationId);
        }

        public async Task<IEnumerable<LocationAccount>> GetLocationsForAccountAsync(string accountId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(LocationAccount), Actions.Read, new SecurityHelper { UserId = accountId });
            return await _locationAccountRepo.GetLocationsForAccountAsync(accountId);
        }

        public async Task<InvokeResult> RemoveAccountFromLocationAsync(string locationId, string userId, EntityHeader org, EntityHeader removedBy)
        {
            await AuthorizeOrgAccessAsync(removedBy, org, typeof(LocationAccount), Actions.Delete, new SecurityHelper { LocationId = locationId, UserId = userId });
            await _locationAccountRepo.RemoveAccountFromLocationAsync(locationId, userId, removedBy);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> AddAccountRoleForLocationAsync(EntityHeader location, EntityHeader user, EntityHeader role, EntityHeader org, EntityHeader addedBy)
        {
            await AuthorizeOrgAccessAsync(addedBy, org, typeof(LocationUserRole), Actions.Read, new SecurityHelper { LocationId = location.Id, UserId = user.Id, RoleId = role.Id });
            var locationUserRole = new LocationUserRole(location, user )
            {
                RoleId = role.Id,
                RoleName = role.Text                 
            };

            await _locationRoleRepo.AddRoleForAccountAsync(locationUserRole);
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
        public async Task<IEnumerable<LocationUserRole>> GetAccountRolesForLocationAsync(string locationId, string userId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(LocationUserRole), Actions.Read, data: new SecurityHelper { LocationId = locationId, UserId = userId });
            return await _locationRoleRepo.GetRolesForAccountInLocationAsync(locationId, userId);
        }

        /// <summary>
        /// Return accounts that fill a specific role in a location.
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="roleId"></param>
        /// <param name="org">permissions</param>
        /// <param name="user">permissions</param>
        /// <returns></returns>
        public async Task<IEnumerable<LocationUserRole>> GetAccountsForRoleInLocationAsync(string locationId, string roleId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(LocationUserRole), Actions.Read, new SecurityHelper { LocationId = locationId, RoleId = roleId });
            return await _locationRoleRepo.GetAccountsForRoleInLocationAsync(locationId, roleId);
        }

        public async Task<InvokeResult> RevokeRoleForAccountInLocationAsync(string locationId, string userId, string roleId, EntityHeader org, EntityHeader revokedBy)
        {
            await AuthorizeOrgAccessAsync(revokedBy, org, typeof(LocationUserRole), action: Actions.Delete, data: new SecurityHelper { LocationId = locationId, UserId = userId, RoleId = roleId });
            await _locationRoleRepo.RevokeRoleForAccountInLocationAsync(locationId, userId, roleId);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> RevokeAllRolesForAccountInLocationAsync(string locationId, string accountId, EntityHeader org, EntityHeader revokedBy)
        {
            await AuthorizeOrgAccessAsync(revokedBy, org, typeof(LocationUserRole), action: Actions.Delete, data: new SecurityHelper { LocationId = locationId, UserId = accountId });
            await _locationRoleRepo.RevokeAllRolesForAccountInLocationAsync(locationId, accountId);
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