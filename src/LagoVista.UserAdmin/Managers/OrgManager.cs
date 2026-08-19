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
using Newtonsoft.Json;
using LagoVista.Core.Models.Geo;

namespace LagoVista.UserAdmin.Managers
{


    public class OrgManager : ManagerBase, IOrganizationManager
    {
        #region Fields
        private readonly IOrganizationRepo _organizationRepo;
        private readonly IOrgLocationRepo _locationRepo;
        private readonly IOrgUserRepo _orgUserRepo;
        private readonly ILocationUserRepo _locationUserRepo;
        private readonly IUserRoleManager _userRoleManager;
        private readonly IUserRoleRepo _userRoleRepo;
        private readonly IEmailSender _emailSender;
        private readonly IInviteUserRepo _inviteUserRepo;
        private readonly ILocationRoleRepo _locationRoleRepo;
        private readonly IAppUserRepo _appUserRepo;
        private readonly ILogger _adminLogger;
        private readonly IOrgInitializer _orgInitializer;
        private readonly IOwnedObjectRepo _ownedObjectRepo;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IDefaultRoleList _defaultRoleList;
        private readonly IAuthenticationLogManager _authLogMgr;
        private readonly ILocationDiagramRepo _diagramRepo;
        private readonly IRoleRepo _roleRepo;
        private readonly IOrgInformationSource _orgInfoSource;
        private readonly ISecureStorage _securestorage; 
        #endregion

        #region Ctor
        public OrgManager(IOrganizationRepo organizationRepo,
            IOrgLocationRepo locationRepo,
            IOrgUserRepo orgUserRepo,
            IAppUserRepo appUserRepo,
            IInviteUserRepo inviteUserRepo,
            ILocationUserRepo locationUserRepo,
            ILocationRoleRepo locationRoleRepo,
            IEmailSender emailSender,
            IOrgInitializer orgInitializer,
            IDefaultRoleList defaultRoleList,            
            IOwnedObjectRepo ownedObjectRepo,
            IUserRoleRepo userRoleRepo,
            IUserRoleManager useRoleManager,
            IAuthenticationLogManager authLogMgr,
            ISubscriptionManager subscriptionManager,
            ILocationDiagramRepo diagramRepo,
            IRoleRepo roleRepo,
            ISecureStorage secureStorage,
            ICoreAppServices coreAppServices,
            IOrgInformationSource orgInfoSource ) : base(coreAppServices)
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
            _emailSender = emailSender;
            _inviteUserRepo = inviteUserRepo;
            _defaultRoleList = defaultRoleList;
            _userRoleRepo = userRoleRepo;
            _adminLogger = coreAppServices.Logger;
            _orgInitializer = orgInitializer;
            _ownedObjectRepo = ownedObjectRepo;
            _diagramRepo = diagramRepo;
            _roleRepo = roleRepo ?? throw new ArgumentNullException(nameof(roleRepo));
            _orgInfoSource = orgInfoSource;
            _securestorage = secureStorage;
        }
        #endregion

        #region Organizations
        public Task<bool> QueryOrgNamespaceInUseAsync(string namespaceText)
        {
            return _orgInfoSource.QueryOrgNamespaceInUseAsync(namespaceText);
        }

        public async Task<InvokeResult<Organization>> CreateNewOrganizationAsync(CreateOrganizationViewModel organizationViewModel, EntityHeader user, string orgId = null)
        {
            var result = new InvokeResult<Organization>();

            ValidationCheck(organizationViewModel, Core.Validation.Actions.Create);

            if (await _organizationRepo.QueryNamespaceInUseAsync(organizationViewModel.Namespace))
            {
                result.Errors.Add(new ErrorMessage(UserAdminResources.Organization_NamespaceInUse.Replace(Tokens.NAMESPACE, organizationViewModel.Namespace)));
                return result;
            }

            var organization = new Organization();
            // Used to force a user id, to only be called by tests.
            if(!String.IsNullOrEmpty(orgId))
                organization.Id = orgId;
            else
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
            organization.DefaultVectorCollectionName = $"{organization.Namespace}-indexes";

            /* Create the Organization in Storage */
            await _organizationRepo.AddOrganizationAsync(organization);

            await _authLogMgr.AddAsync(AuthLogTypes.OrganizationCreationStarted, user.Id, user.Text, organization.Id, organization.Name);

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
            await _authLogMgr.AddAsync(AuthLogTypes.OrganizationPopulationStarted, user.Id, user.Text, organization.Id, organization.Name);
            await _orgInitializer.Init(organization.ToEntityHeader(), currentUser.ToEntityHeader(), organizationViewModel.CreateGettingStartedData);
            await _authLogMgr.AddAsync(AuthLogTypes.OrganizationPopulationSucceeded, user.Id, user.Text, organization.Id, organization.Name);
            //});

            await LogEntityActionAsync(organization.Id, typeof(Organization).Name, "Created Organization", organization.ToEntityHeader(), user);

            await _authLogMgr.AddAsync(AuthLogTypes.OrganizationCreationSucceeded, user.Id, user.Text, organization.Id, organization.Name);

            return InvokeResult<Organization>.Create(organization);
        }

        public async Task<InvokeResult<Organization>> CreateProvisionalOrganizationAsync(AppUser appUser, string organizationId)
        {
            if (appUser == null) throw new ArgumentNullException(nameof(appUser));
            if (String.IsNullOrEmpty(organizationId)) throw new ArgumentNullException(nameof(organizationId));

            var user = EntityHeader.Create(appUser.Id, appUser.UserName);
            var createdOrganization = false;
            var organization = await _organizationRepo.QueryOrganizationExistAsync(organizationId)
                ? await _organizationRepo.GetOrganizationAsync(organizationId)
                : null;
            if (organization == null)
            {
                var provisionalNamespace = $"provisional{organizationId.ToLowerInvariant()}";
                organization = new Organization
                {
                    Id = organizationId,
                    Name = "Provisional Workspace",
                    Namespace = provisionalNamespace,
                    DefaultVectorCollectionName = $"{provisionalNamespace}-indexes",
                    Status = UserAdminResources.Organization_Status_Active,
                    TechnicalContact = user,
                    AdminContact = user,
                    BillingContact = user,
                    DefaultProjectLead = user,
                    DefaultProjectAdminLead = user,
                    DefaultContributor = user,
                    DefaultQAResource = user,
                    Owner = user,
                    IsForProductLine = false
                };

                organization.SetCreationUpdatedFields(user);
                await _organizationRepo.AddOrganizationAsync(organization);
                createdOrganization = true;
            }
            else
            {
                await _organizationRepo.EnsureRelationalOrganizationAsync(organization);
            }

            var ownerRoleId = _defaultRoleList.GetStandardRoles().Single(role => role.Key == DefaultRoleList.OWNER).Id;
            if (createdOrganization || !await _userRoleManager.UserHasRoleAsync(ownerRoleId, appUser.Id, organization.Id))
            {
                await _userRoleManager.GrantUserRoleAsync(appUser.Id, ownerRoleId, organization.ToEntityHeader(), user);
            }

            var addUserResult = await EnsureProvisionalUserMembershipAsync(appUser, organization, user, createdOrganization);
            if (!addUserResult.Successful) return InvokeResult<Organization>.FromInvokeResult(addUserResult);

            if (appUser.Organizations == null) appUser.Organizations = new List<EntityHeader>();
            if (!appUser.Organizations.Any(org => org.Id == organization.Id)) appUser.Organizations.Add(organization.ToEntityHeader());
            appUser.IsOrgAdmin = true;
            appUser.IsAppBuilder = true;
            appUser.CurrentOrganization = organization.CreateSummary();
            await _appUserRepo.UpdateAsync(appUser);

            return InvokeResult<Organization>.Create(organization);
        }

        private async Task<InvokeResult> EnsureProvisionalUserMembershipAsync(AppUser appUser, Organization organization, EntityHeader addedBy, bool skipExistenceCheck = false)
        {
            if (!skipExistenceCheck && await _orgUserRepo.QueryOrgHasUserAsync(organization.Id, appUser.Id)) return InvokeResult.Success;

            var timeStamp = UtcTimestamp.Now;
            var orgUser = new OrgUser(organization.Id, appUser.Id)
            {
                Email = appUser.Email,
                OrganizationName = organization.Name,
                UserName = appUser.Name,
                IsOrgAdmin = true,
                IsAppBuilder = true,
                ProfileImageUrl = appUser.ProfileImage.ImageUrl,
                CreatedBy = appUser.Name,
                CreatedById = appUser.Id,
                CreationDate = timeStamp,
                LastUpdatedBy = appUser.Name,
                LastUpdatedById = appUser.Id,
                LastUpdatedDate = timeStamp
            };

            try
            {
                await _orgUserRepo.AddOrgUserAsync(orgUser);
            }
            catch
            {
                if (!await _orgUserRepo.QueryOrgHasUserAsync(organization.Id, appUser.Id)) throw;
                return InvokeResult.Success;
            }

            await _authLogMgr.AddAsync(AuthLogTypes.AddUserToOrg, appUser.Id, appUser.Name, organization.Id, organization.Name, extras: $"added by id: {addedBy.Id}, name: {addedBy.Text}");

            if (appUser.CurrentOrganization == null)
            {
                appUser.CurrentOrganization = organization.CreateSummary();
                appUser.LastUpdatedBy = addedBy;
                appUser.LastUpdatedDate = timeStamp;
                appUser.AddChange(nameof(AppUser.CurrentOrganization), "none", appUser.CurrentOrganization.Text);
            }

            return InvokeResult.Success;
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

            if(!String.IsNullOrEmpty(org.VimeoAccessTokenSecret))
            {
                if(!String.IsNullOrEmpty(org.VimeoAccessTokenSecretId))
                {
                    var result = await _securestorage.RemoveSecretAsync(userOrg, org.VimeoAccessTokenSecretId);
                    if (!result.Successful) return result;
                }

                var addResult = await _securestorage.AddSecretAsync(userOrg, org.VimeoAccessTokenSecret);
                if (!addResult.Successful) return addResult.ToInvokeResult();

                org.VimeoAccessTokenSecretId = addResult.Result;
                org.VimeoAccessTokenSecret = null;
            }

            await AuthorizeAsync(org, AuthorizeResult.AuthorizeActions.Update, user, userOrg);
            await _organizationRepo.UpdateOrganizationAsync(org);

            return InvokeResult.Success;
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

        public Task<string> GetOrgNameAsync(string orgId)
        {
            return _orgInfoSource.GetOrgNameAsync(orgId);
        }

        public Task<string> GetOrgNameSpaceAsync(string orgId)
        {
            return _orgInfoSource.GetOrgNameSpaceAsync(orgId);
        }

        public Task<string> GetOrgIdForNameSpaceAsync(string orgNameSpace)
        {
            return _orgInfoSource.GetOrgIdForNameSpaceAsync(orgNameSpace);
        }

        public Task<PublicOrgInformation> GetPublicOrginfoAsync(string orgns)
        {
            return _orgInfoSource.GetPublicOrginfoAsync(orgns);
        }

        public Task<EntityHeader> GetOrgEntityHeaderForNameSpaceAsync(string orgNameSpace)
        {
            return _orgInfoSource.GetOrgEntityHeaderForNameSpaceAsync(orgNameSpace);
        }

        public Task<InvokeResult<BasicTheme>> GetBasicThemeForOrgAsync(string orgid)
        {
            return _orgInfoSource.GetBasicThemeForOrgAsync(orgid);
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
