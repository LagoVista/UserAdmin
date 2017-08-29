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

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IOrganizationManager
    {
        Task<InvokeResult> AddUserToOrgAsync(String orgId, String userId, EntityHeader userOrg, EntityHeader addedBy);
        Task<InvokeResult> CreateOrganizationAsync(Organization newOrg, EntityHeader userOrg, EntityHeader user);
        Task<InvokeResult> UpdateOrganizationAsync(Organization org, EntityHeader userOrg, EntityHeader user);

        Task<InvokeResult> CreateLocationAsync(OrgLocation location, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateLocationAsync(OrgLocation location, EntityHeader org, EntityHeader user);


        Task<InvokeResult> AcceptInvitationAsync(AcceptInviteViewModel acceptInviteViewModel, string acceptedUserId);
        Task<InvokeResult> AcceptInvitationAsync(string inviteId, EntityHeader orgHeader, EntityHeader user);
        Task<InvokeResult> AddUserRoleForLocationAsync(EntityHeader location, EntityHeader user, EntityHeader role, EntityHeader org, EntityHeader addedBy);
        Task<InvokeResult> AddUserToOrgAsync(EntityHeader userToAdd, EntityHeader org, EntityHeader addedBy);
        Task<InvokeResult> AddUserToLocationAsync(string userId, string locationId, EntityHeader org, EntityHeader addedBy);
        Task<InvokeResult> AddUserRoleForOrgAsync(EntityHeader org, EntityHeader user, EntityHeader role, EntityHeader addedByOrg, EntityHeader addedBy);
        Task<InvokeResult> AddLocationAsync(CreateLocationViewModel newLocation, EntityHeader org, EntityHeader user);
        Task<InvokeResult> CreateNewOrganizationAsync(CreateOrganizationViewModel organizationViewModel, EntityHeader user);

        Task DeclineInvitationAsync(String inviteId);
        CreateLocationViewModel GetCreateLocationViewModel(EntityHeader org, EntityHeader user);
        Task<IEnumerable<LocationUserRole>> GetRolesForUserInLocationAsync(string locationId, string userId, EntityHeader org, EntityHeader user);
        Task<IEnumerable<OrganizationUserRole>> GetUsersRolesInOrgAsync(string orgId, string userId, EntityHeader org, EntityHeader user);
        Task<IEnumerable<LocationUser>> GetUsersForLocationAsync(string locationId, EntityHeader org, EntityHeader user);
        Task<IEnumerable<UserInfoSummary>> GetUsersForOrganizationsAsync(string orgId, EntityHeader org, EntityHeader user);
        Task<IEnumerable<LocationUserRole>> GetUserWithRoleInLocationAsync(string locationId, string roleId, EntityHeader org, EntityHeader user);
        Task<IEnumerable<OrganizationUserRole>> GetUserWithRoleInOrgAsync(string orgId, string roleId, EntityHeader org, EntityHeader user);
        Task<AcceptInviteViewModel> GetInviteViewModelAsync(string inviteId);
        Task<IEnumerable<OrgLocation>> GetLocationsForOrganizationsAsync(string orgId, EntityHeader org, EntityHeader user);
        Task<IEnumerable<LocationUser>> GetLocationsForUserAsync(string userId, EntityHeader org, EntityHeader user);
        Task<Organization> GetOrganizationAsync(string ogId, EntityHeader org, EntityHeader user);
        Task<IEnumerable<OrgUser>> GetOrganizationsForUserAsync(string userId, EntityHeader org, EntityHeader user);
        Task<UpdateLocationViewModel> GetUpdateLocationViewModelAsync(string locationId, EntityHeader org, EntityHeader user);
        Task<UpdateOrganizationViewModel> GetUpdateOrganizationViewModel(string orgId, EntityHeader org, EntityHeader user);
        Task<InvokeResult<Invitation>> InviteUserAsync(InviteUser inviteViewModel, EntityHeader orgEntityHeader, EntityHeader userEntityHeader);

        Task<bool> QueryLocationNamespaceInUseAsync(string orgId, string namespaceText);
        Task<bool> QueryOrganizationHasUserAsync(string orgId, string userId, EntityHeader org, EntityHeader user);
        Task<bool> QueryOrgNamespaceInUseAsync(string namespaceText);

        Task<InvokeResult> RemoveUserFromLocationAsync(String locationId, String userId, EntityHeader org, EntityHeader removedBy);
        Task<InvokeResult> RemoveUserFromOrganizationAsync(string orgId, string userId, EntityHeader org, EntityHeader removedBy);


        Task<InvokeResult> RevokeAllRolesForUserInLocationAsync(string locationId, string userId, EntityHeader org, EntityHeader revokedBy);
        Task<InvokeResult> RevokeAllRolesForUserInOrgAsync(string locationId, string userId, EntityHeader org, EntityHeader revokedBy);
        Task<InvokeResult> RevokeRoleForUserInLocationAsync(string locationId, string userId, string roleId, EntityHeader org, EntityHeader revokedBy);
        Task<InvokeResult> RevokeRoleForUserInOrgAsync(string orgId, string userId, string roleId, EntityHeader userOrg, EntityHeader revokedBy);
        Task<InvokeResult> RevokeInvitationAsync(string inviteId, EntityHeader org, EntityHeader user);


        Task<InvokeResult> UpdateLocationAsync(UpdateLocationViewModel location, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateOrganizationAsync(UpdateOrganizationViewModel orgViewModel, EntityHeader org, EntityHeader user);
    }
}
