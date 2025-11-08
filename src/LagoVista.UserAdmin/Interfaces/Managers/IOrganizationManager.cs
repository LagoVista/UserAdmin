// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 91f8226c0a63ee141df21c5c6cba92548088798859efd58e56ac1b0c60f37e91
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.ViewModels.Organization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.Core.Models.Geo;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IOrganizationManager
    {
        Task<InvokeResult> AddUserToOrgAsync(String orgId, String userId, EntityHeader userOrg, EntityHeader addedBy);
        Task<InvokeResult> CreateOrganizationAsync(Organization newOrg, EntityHeader userOrg, EntityHeader user);
        Task<InvokeResult> UpdateOrganizationAsync(Organization org, EntityHeader userOrg, EntityHeader user);



        Task<InvokeResult<AcceptInviteResponse>> AcceptInvitationAsync(string inviteId, AppUser appoUser);
        Task<InvokeResult<AcceptInviteResponse>> AcceptInvitationAsync(string inviteId, string appoUserId);

        Task<InvokeResult> AddUserToOrgAsync(EntityHeader userToAdd, EntityHeader org, EntityHeader addedBy, bool isOrgAdmin = false, bool isAppBuilder = false);
        Task<InvokeResult> AddUserToLocationAsync(string userId, string locationId, EntityHeader org, EntityHeader addedBy);
        Task<InvokeResult> AddLocationAsync(OrgLocation newLocation, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateLocationAsync(OrgLocation location, EntityHeader org, EntityHeader user);
        Task<InvokeResult<Organization>> CreateNewOrganizationAsync(CreateOrganizationViewModel organizationViewModel, EntityHeader user);
        Task<InvokeResult<List<GeoLocation>>> GetBoundingBoxForLocationAsync(string orgLocation, EntityHeader org, EntityHeader user);

        Task<InvokeResult> SetOrgAdminAsync(string userId, EntityHeader org, EntityHeader user);

        Task<InvokeResult> ClearOrgAdminAsync(string userId, EntityHeader org, EntityHeader user);

        Task<InvokeResult> SetAppBuilderAsync(string userId, EntityHeader org, EntityHeader user);

        Task<InvokeResult> ClearAppBuilderAsync(string userId, EntityHeader org, EntityHeader user);

        Task<InvokeResult> DeleteOrgAsync(string orgId, EntityHeader org, EntityHeader user);

        Task<ListResponse<OwnedObject>> GetOwnedObjectsForOrgAsync(string orgId, ListRequest request, EntityHeader org, EntityHeader user);

        Task<PublicOrgInformation> GetPublicOrginfoAsync(string orgns);

        Task<bool> IsUserOrgAdminAsync(string orgId, string userId);

        Task<bool> IsUserAppBuildernAsync(string orgId, string userId);

        Task<InvokeResult<AppUser>> ChangeOrgsAsync(string newOrgId, EntityHeader org, EntityHeader user);
        Task<InvokeResult<AppUser>> ChangeOrgsAsync(string newOrgId, EntityHeader org, AppUser user);

        Task DeclineInvitationAsync(String inviteId);
        Task<IEnumerable<LocationUserRole>> GetRolesForUserInLocationAsync(string locationId, string userId, EntityHeader org, EntityHeader user);
        Task<IEnumerable<LocationUser>> GetUsersForLocationAsync(string locationId, EntityHeader org, EntityHeader user);
        Task<IEnumerable<UserInfoSummary>> GetUsersForOrganizationsAsync(string orgId, bool useCache, EntityHeader org, EntityHeader user);
        Task<IEnumerable<UserInfoSummary>> GetActiveUsersForOrganizationsAsync(string orgId, bool useCache, EntityHeader org, EntityHeader user);
        Task<IEnumerable<LocationUserRole>> GetUserWithRoleInLocationAsync(string locationId, string roleId, EntityHeader org, EntityHeader user);
        Task<AcceptInviteViewModel> GetInviteViewModelAsync(string inviteId);

        Task<ListResponse<Invitation>> GetInvitationsAsync(ListRequest request, EntityHeader org, EntityHeader user, Invitation.StatusTypes? byStatus = null);
        Task<ListResponse<Invitation>> GetActiveInvitationsForOrgAsync(ListRequest request, EntityHeader org, EntityHeader user);
        Task<bool> GetIsInvigationActiveAsync(string inviteId);

        Task<ListResponse<OrganizationSummary>> GetAllOrgsAsync(EntityHeader org, EntityHeader user, ListRequest listRequest);
        Task<ListResponse<OrganizationSummary>> SearchAllOrgsAsync(string searchOrgName, EntityHeader user, ListRequest listRequest);

        Task<Invitation> GetInvitationAsync(string inviteId);
       
        Task<InvokeResult> ResendInvitationAsync(string inviteId, EntityHeader org, EntityHeader user);

        Task<ListResponse<OrgLocationSummary>> GetLocationsForOrganizationsAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<ListResponse<OrgLocationSummary>> GetLocationsForCustomerAsync(ListRequest listRequest, string customerId, EntityHeader org, EntityHeader user);
        Task<OrgLocation> GetOrgLocationAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteOrgLocationAsync(string id, EntityHeader org, EntityHeader user);
        Task<IEnumerable<LocationUser>> GetLocationsForUserAsync(string userId, EntityHeader org, EntityHeader user);
        Task<Organization> GetOrganizationAsync(string ogId, EntityHeader org, EntityHeader user);
        Task<IEnumerable<OrgUser>> GetOrganizationsForUserAsync(string userId, EntityHeader org, EntityHeader user);

        Task SetDefaultOrguserRoleAsync(string orgId, string userId, string roleId, EntityHeader userOrg, EntityHeader setByUser);
        Task ClearDefaultOrgUserRoleAsync(string orgId, string userId, EntityHeader userOrg, EntityHeader setByUser);

        Task<InvokeResult> ClearOrgUserCache(EntityHeader org, EntityHeader user);
        Task<UpdateOrganizationViewModel> GetUpdateOrganizationViewModel(string orgId, EntityHeader org, EntityHeader user);
        Task<InvokeResult<Invitation>> InviteUserAsync(InviteUser inviteViewModel, EntityHeader orgEntityHeader, EntityHeader userEntityHeader);

        Task<bool> QueryLocationNamespaceInUseAsync(string orgId, string namespaceText);
        Task<bool> QueryOrganizationHasUserAsync(string orgId, string userId, EntityHeader org, EntityHeader user);
        Task<bool> QueryOrgNamespaceInUseAsync(string namespaceText);

        Task<InvokeResult> RemoveUserFromLocationAsync(String locationId, String userId, EntityHeader org, EntityHeader removedBy);
        Task<InvokeResult> RemoveUserFromOrganizationAsync(string orgId, string userId, EntityHeader org, EntityHeader removedBy);

        Task<InvokeResult> RevokeInvitationAsync(string inviteId, EntityHeader org, EntityHeader user);

        Task<InvokeResult<string>> GetLandingPageForOrgAsync(string orgid);

        Task<InvokeResult<Organization>> SysAdminGetOrgAsync(string orgId, EntityHeader user);
        Task<InvokeResult> SysAdminUpdateOrgAsync(Organization org, EntityHeader user);

        Task<InvokeResult> SetIsProductLineOrgAsync(bool isProductLineOrg, EntityHeader org, EntityHeader user);

        Task<string> GetOrgNameAsync(string orgId);
        Task<string> GetOrgNameSpaceAsync(string orgId);
        Task<string> GetOrgIdForNameSpaceAsync(string orgNameSpace);

        Task<EntityHeader> GetOrgEntityHeaderForNameSpaceAsync(string orgNameSpace);

        Task<InvokeResult<BasicTheme>> GetBasicThemeForOrgAsync(string orgid);
    }
}
