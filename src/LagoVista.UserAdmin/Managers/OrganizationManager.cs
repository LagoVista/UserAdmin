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
            ValidationCheck(organizationViewModel, Core.Validation.Actions.Create);

            //HACK: Very, small chance, but it does exist...two entries could be added at exact same time and check would fail...need to make progress, can live with rish for now.
            if (await _organizationRepo.QueryNamespaceInUseAsync(organizationViewModel.Namespace))
            {
                throw new ValidationException(Resources.UserAdminResources.Organization_CantCreate, false,
                                   UserAdminResources.Organization_NamespaceInUse.Replace(Tokens.NAMESPACE, organizationViewModel.Namespace));
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

            await _organizationRepo.AddOrganizationAsync(organization);
            await AddAccountToOrgAsync(currentUser.ToEntityHeader(), organization.ToEntityHeader(), currentUser.ToEntityHeader());
            await _locationRepo.AddLocationAsync(location);
            await _locationAccountRepo.AddAccountToLocationAsync(locationUser);

            if (EntityHeader.IsNullOrEmpty(currentUser.CurrentOrganization)) currentUser.CurrentOrganization = organization.ToEntityHeader();
            if (currentUser.Organizations == null) currentUser.Organizations = new List<EntityHeader>();

            currentUser.Organizations.Add(organization.ToEntityHeader());

            await _appUserRepo.UpdateAsync(currentUser);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> CreateOrganizationAsync(Organization newOrg, EntityHeader userOrg, EntityHeader user)
        {
            /* This means that the user is creating the org upon sign up, 
             * just go ahead and assign this org as the owner org. */
            if(userOrg == null)
            {
                newOrg.OwnerOrganization = newOrg.ToEntityHeader();
            }

            await AuthorizeAsync(newOrg, AuthorizeResult.AuthorizeActions.Create, user, userOrg);
            ValidationCheck(newOrg, Core.Validation.Actions.Create);
            await _organizationRepo.AddOrganizationAsync(newOrg);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateOrganizationAsync(Organization org, EntityHeader userOrg, EntityHeader user)
        {
            ValidationCheck(org, Core.Validation.Actions.Create);
            await AuthorizeAsync(org, AuthorizeResult.AuthorizeActions.Update, user, userOrg);
            await _organizationRepo.AddOrganizationAsync(org);

            await AddAccountToOrgAsync(user, org.ToEntityHeader(), user);

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
            ValidationCheck(location, Core.Validation.Actions.Create);
            await AuthorizeAsync(location, AuthorizeResult.AuthorizeActions.Update, user, org);
            await _locationRepo.UpdateLocationAsync(location);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateOrganizationAsync(UpdateOrganizationViewModel orgViewModel, EntityHeader user)
        {
            ValidationCheck(orgViewModel, Core.Validation.Actions.Update);

            var org = await _organizationRepo.GetOrganizationAsync(orgViewModel.OrganziationId);
            org.SetLastUpdatedFields(user);
            ConcurrencyCheck(org, orgViewModel.LastUpdatedDate);

            await _organizationRepo.UpdateOrganizationAsync(org);

            return InvokeResult.Success;
        }

        public async Task<UpdateOrganizationViewModel> GetUpdateOrganizationViewModel(string organizationId)
        {
            var org = await _organizationRepo.GetOrganizationAsync(organizationId);
            return UpdateOrganizationViewModel.CreateFromOrg(org);
        }

        public Task<Organization> GetOrganizationAsync(string organizationId)
        {
            return _organizationRepo.GetOrganizationAsync(organizationId);
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

            await AddAccountToOrgAsync(acceptedUser.ToEntityHeader(), orgHeader, invitingUser);

            if (acceptedUser.CurrentOrganization == null || acceptedUser.CurrentOrganization.IsEmpty())
            {
                acceptedUser.CurrentOrganization = orgHeader;
            }

            acceptedUser.Organizations.Add(orgHeader);

            await _appUserRepo.UpdateAsync(acceptedUser);

            return InvokeResult.Success;
        }

        public async Task RevokeInvitationAsync(String inviteId)
        {
            var invite = await _inviteUserRepo.GetInvitationAsync(inviteId);
            invite.Status = Invitation.StatusTypes.Revoked;
            await _inviteUserRepo.UpdateInvitationAsync(invite);
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

        public async Task<Invitation> InviteUserAsync(InviteUserViewModel inviteViewModel, EntityHeader orgEntityHeader, EntityHeader userEntityHeader)
        {
            if (await _orgAccountRepo.QueryOrganizationHasAccountByEmailAsync(orgEntityHeader.Id, inviteViewModel.Email))
            {
                var existingUser = await _appUserRepo.FindByEmailAsync(inviteViewModel.Email);
                var msg = UserAdminResources.InviteUser_AlreadyPartOfOrg.Replace(Tokens.USERS_FULL_NAME, existingUser.Name).Replace(Tokens.EMAIL_ADDR, inviteViewModel.Email);
                throw new ValidationException("Could not invite user", false, msg);
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

            return inviteModel;
        }
        #endregion

        #region Organization Account Methods
        public async Task<InvokeResult> AddAccountToOrgAsync(EntityHeader userToAdd, EntityHeader org, EntityHeader addedBy = null)
        {
            var result = InvokeResult.Success;
            var appUser = await _appUserRepo.FindByIdAsync(userToAdd.Id);

            if (await _orgAccountRepo.QueryOrganizationHasAccountAsync(org.Id, userToAdd.Id))
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

            accountUser.CreatedBy = addedBy != null ? addedBy.Text : userToAdd.Text;
            accountUser.CreatedById = addedBy != null ? addedBy.Id : userToAdd.Id;
            accountUser.CreationDate = DateTime.Now.ToJSONString();
            accountUser.LastUpdatedBy = appUser.Name;
            accountUser.LastUpdatedById = appUser.Id;
            accountUser.LastUpdatedDate = accountUser.CreationDate;
            await _orgAccountRepo.AddAccountUserAsync(accountUser);

            return result;
        }

        public async Task<InvokeResult> AddAccountToOrgAsync(String orgId, String userId, EntityHeader addedBy)
        {
            var appUser = await _appUserRepo.FindByIdAsync(userId);

            var org = await _organizationRepo.GetOrganizationAsync(orgId);


            await AuthorizeOrgAccess(addedBy, org.ToEntityHeader(), typeof(OrganizationAccount));

            if (await _orgAccountRepo.QueryOrganizationHasAccountAsync(orgId, userId))
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
            accountUser.LastUpdatedBy = appUser.Name;
            accountUser.LastUpdatedById = appUser.Id;
            accountUser.LastUpdatedDate = accountUser.CreationDate;
            await _orgAccountRepo.AddAccountUserAsync(accountUser);

            return InvokeResult.Success;
        }


        public Task<IEnumerable<OrganizationAccount>> GetAccountsForOrganizationsAsync(string orgId)
        {
            return _orgAccountRepo.GetAccountsForOrganizationAsync(orgId);
        }

        public Task<IEnumerable<OrganizationAccount>> GetOrganizationsForAccountAsync(string accountId)
        {
            return _orgAccountRepo.GetOrganizationsForAccountAsync(accountId);
        }

        public async Task<InvokeResult> RemoveAccountFromOrganizationAsync(string orgId, string userId, EntityHeader removedBy)
        {
            await _orgAccountRepo.RemoveAccountFromOrgAsync(orgId, userId, removedBy);
            return InvokeResult.Success;

        }

        public Task<bool> QueryOrganizationHasAccountAsync(string orgId, string accountId)
        {
            return _orgAccountRepo.QueryOrganizationHasAccountAsync(orgId, accountId);
        }
        #endregion

        #region Organization Location
        public async Task<bool> QueryLocationNamespaceInUseAsync(string orgId, string namespaceText)
        {
            return await _locationRepo.QueryNamespaceInUseAsync(orgId, namespaceText);
        }

        public async Task<IEnumerable<OrganizationLocation>> GetLocationsForOrganizationsAsync(String orgId)
        {
            return (await _locationRepo.GetOrganizationLocationAsync(orgId)).ToList();
        }

        public async Task<InvokeResult> AddLocationAsync(CreateLocationViewModel newLocation, EntityHeader addedByUser)
        {
            ValidationCheck(newLocation, Core.Validation.Actions.Create);

            var location = new OrganizationLocation();
            location.SetId();

            var currentUser = await _appUserRepo.FindByIdAsync(addedByUser.Id);
            var organization = await _organizationRepo.GetOrganizationAsync(newLocation.OrganizationId);

            location.Organization = organization.ToEntityHeader();
            if (EntityHeader.IsNullOrEmpty(location.AdminContact)) location.AdminContact = addedByUser;
            if (EntityHeader.IsNullOrEmpty(location.TechnicalContact)) location.TechnicalContact = addedByUser;

            SetCreatedBy(location, addedByUser);

            await _locationRepo.AddLocationAsync(location);

            return InvokeResult.Success;
        }

        public CreateLocationViewModel GetCreateLocationViewModel(EntityHeader org, EntityHeader user)
        {
            return CreateLocationViewModel.CreateNew(org, user);
        }

        public async Task<UpdateLocationViewModel> GetUpdateLocationViewModelAsync(String locationId)
        {
            var location = await _locationRepo.GetLocationAsync(locationId);
            return UpdateLocationViewModel.CreateForOrganizationLocation(location);
        }

        public async Task<InvokeResult> UpdateLocationAsync(UpdateLocationViewModel location, EntityHeader user)
        {
            ValidationCheck(location, Core.Validation.Actions.Update);

            var locationFromStorage = await _locationRepo.GetLocationAsync(location.LocationId);
            ConcurrencyCheck(locationFromStorage, location.LastUpdatedDate);
            SetLastUpdated(locationFromStorage, user);

            await _locationRepo.UpdateLocationAsync(locationFromStorage);

            return InvokeResult.Success;
        }

        public Task<OrganizationAccountRoles> AddAccountRoleForOrgAsync(EntityHeader location, EntityHeader account, EntityHeader role, EntityHeader addedBy)
        {
            return _orgRoleRepo.AddRoleForAccountAsync(location, account, role, addedBy);
        }

        public Task<IEnumerable<OrganizationAccountRoles>> GetAccountRolesForOrgAsync(string orgId, string accountId)
        {
            return _orgRoleRepo.GetRolesForAccountAsync(accountId, orgId);
        }

        public Task<IEnumerable<OrganizationAccountRoles>> GetAccountsForRoleInOrgAsync(string orgId, string roleId)
        {
            return _orgRoleRepo.GetAccountsForRoleAsync(orgId, roleId);
        }

        public Task RevokeRoleForAccountInOrgAsync(EntityHeader org, EntityHeader role, EntityHeader account, EntityHeader revokedBy)
        {
            return _orgRoleRepo.RevokeRoleForAccountInOrgAsync(org, role, account, revokedBy);
        }

        public Task RevokeAllRolesForAccountInOrgAsync(EntityHeader org, EntityHeader account, EntityHeader revokedBy)
        {
            return _orgRoleRepo.RevokeAllRolesForAccountInOrgAsync(org, account, revokedBy);
        }
        #endregion

        #region Location Account
        public async Task<InvokeResult> AddAccountToLocationAsync(String accountId, String locationId, EntityHeader addedBy = null)
        {
            var appUser = await _appUserRepo.FindByIdAsync(accountId);
            var location = await _locationRepo.GetLocationAsync(locationId);

            var locationAccount = new LocationAccount(location.Organization.Id, locationId, accountId)
            {
                AccountName = appUser.Name
            };

            await _locationAccountRepo.AddAccountToLocationAsync(locationAccount);

            return InvokeResult.Success;
        }

        public Task GetAccountsForLocationAsync(String locationId)
        {
            return _locationAccountRepo.GetAccountsForLocationAsync(locationId);
        }

        public Task<IEnumerable<LocationAccount>> GetLocationsForAccountAsync(String accountId)
        {
            return _locationAccountRepo.GetLocationsForAccountAsync(accountId);
        }

        public async Task<InvokeResult> RemoveAccountFromLocation(String locationId, String accountId, EntityHeader removedBy)
        {
            await _locationAccountRepo.RemoveAccountFromLocationAsync(locationId, accountId, removedBy);

            return InvokeResult.Success;
        }

        public Task<LocationAccountRoles> AddAccountRoleForLocationAsync(EntityHeader location, EntityHeader account, EntityHeader role, EntityHeader addedBy)
        {
            return _locationRoleRepo.AddRoleForAccountAsync(location, account, role, addedBy);
        }

        public Task<IEnumerable<LocationAccountRoles>> GetAccountRolesForLocationAsync(string locationId, string accountId)
        {
            return _locationRoleRepo.GetRolesForAccountInLocationAsync(locationId, accountId);
        }

        public Task<IEnumerable<LocationAccountRoles>> GetAccountsForRoleInLocationAsync(string locationId, string roleId)
        {
            return _locationRoleRepo.GetAccountsForRoleInLocationAsync(locationId, roleId);
        }

        public Task RevokeRoleForAccountInLocationAsync(EntityHeader location, EntityHeader role, EntityHeader account, EntityHeader revokedBy)
        {
            return _locationRoleRepo.RevokeRoleForAccountInLocationAsync(location, role, account, revokedBy);
        }

        public Task RevokeAllRolesForAccountInLocationAsync(EntityHeader location, EntityHeader account, EntityHeader revokedBy)
        {
            return _locationRoleRepo.RevokeAllRolesForAccountInLocationAsync(location, account, revokedBy);
        }
        #endregion
    }
}