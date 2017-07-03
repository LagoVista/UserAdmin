using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Account;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.ViewModels.Organization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IOrganizationManager
    {
        Task<InvokeResult> AddAccountToOrgAsync(String orgId, String userId, EntityHeader userOrg, EntityHeader addedBy);
        Task<InvokeResult> CreateOrganizationAsync(Organization newOrg, EntityHeader userOrg, EntityHeader user);
        Task<InvokeResult> UpdateOrganizationAsync(Organization org, EntityHeader userOrg, EntityHeader user);
        
        Task<InvokeResult> CreateLocationAsync(OrganizationLocation location, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateLocationAsync(OrganizationLocation location, EntityHeader org, EntityHeader user);


        Task<InvokeResult> AcceptInvitationAsync(AcceptInviteViewModel acceptInviteViewModel, string acceptedUserId);
        Task<InvokeResult> AddAccountRoleForLocationAsync(EntityHeader location, EntityHeader user, EntityHeader role, EntityHeader org, EntityHeader addedBy);
        Task<InvokeResult> AddAccountToOrgAsync(EntityHeader userToAdd, EntityHeader org, EntityHeader addedBy);
        Task<InvokeResult> AddAccountToLocationAsync(string userId, string locationId, EntityHeader org, EntityHeader addedBy);
        Task<InvokeResult> AddAccountRoleForOrgAsync(EntityHeader org, EntityHeader user, EntityHeader role, EntityHeader addedByOrg, EntityHeader addedBy);
        Task<InvokeResult> AddLocationAsync(CreateLocationViewModel newLocation, EntityHeader org,  EntityHeader user);
        Task<InvokeResult> CreateNewOrganizationAsync(CreateOrganizationViewModel organizationViewModel, EntityHeader user);

        Task DeclineInvitationAsync(String inviteId);
        CreateLocationViewModel GetCreateLocationViewModel(EntityHeader org, EntityHeader user);
        Task<IEnumerable<LocationUserRole>> GetAccountRolesForLocationAsync(string locationId, string userId, EntityHeader org, EntityHeader user);
        Task<IEnumerable<OrganizationUserRole>> GetAccountRolesForOrgAsync(string orgId, string userId, EntityHeader org, EntityHeader user);
        Task<IEnumerable<LocationAccount>> GetAccountsForLocationAsync(string locationId, EntityHeader org, EntityHeader user);
        Task<IEnumerable<OrganizationAccount>> GetUsersForOrganizationsAsync(string orgId, EntityHeader org, EntityHeader user);
        Task<IEnumerable<LocationUserRole>> GetAccountsForRoleInLocationAsync(string locationId, string roleId, EntityHeader org, EntityHeader user);
        Task<IEnumerable<OrganizationUserRole>> GetAccountsForRoleInOrgAsync(string orgId, string roleId, EntityHeader org, EntityHeader user);
        Task<AcceptInviteViewModel> GetInviteViewModelAsync(string inviteId);
        Task<IEnumerable<OrganizationLocation>> GetLocationsForOrganizationsAsync(string orgId, EntityHeader org, EntityHeader user);
        Task<IEnumerable<LocationAccount>> GetLocationsForAccountAsync(string userId, EntityHeader org, EntityHeader user);
        Task<Organization> GetOrganizationAsync(string organizationId, EntityHeader org, EntityHeader user);
        Task<IEnumerable<OrganizationAccount>> GetOrganizationsForAccountAsync(string userId, EntityHeader org, EntityHeader user);
        Task<UpdateLocationViewModel> GetUpdateLocationViewModelAsync(string locationId, EntityHeader org, EntityHeader user);
        Task<UpdateOrganizationViewModel> GetUpdateOrganizationViewModel(string organizationId, EntityHeader org, EntityHeader user);
        Task<InvokeResult<Invitation>> InviteUserAsync(InviteUserViewModel inviteViewModel, EntityHeader orgEntityHeader, EntityHeader userEntityHeader);

        Task<bool> QueryLocationNamespaceInUseAsync(string orgId, string namespaceText);
        Task<bool> QueryOrganizationHasAccountAsync(string orgId, string userId, EntityHeader org, EntityHeader user);
        Task<bool> QueryOrgNamespaceInUseAsync(string namespaceText);

        Task<InvokeResult> RemoveAccountFromLocationAsync(String locationId, String userId, EntityHeader org, EntityHeader removedBy);
        Task<InvokeResult> RemoveUserFromOrganizationAsync(string orgId, string userId, EntityHeader org, EntityHeader removedBy);


        Task<InvokeResult> RevokeAllRolesForAccountInLocationAsync(string locationId, string userId, EntityHeader org, EntityHeader revokedBy);
        Task<InvokeResult> RevokeAllRolesForAccountInOrgAsync(string locationId, string userId, EntityHeader org, EntityHeader revokedBy);
        Task<InvokeResult> RevokeRoleForAccountInLocationAsync(string locationId, string userId, string roleId, EntityHeader org, EntityHeader revokedBy);
        Task<InvokeResult> RevokeRoleForAccountInOrgAsync(string orgId, string userId, string roleId, EntityHeader userOrg, EntityHeader revokedBy);
        Task<InvokeResult> RevokeInvitationAsync(string inviteId, EntityHeader org, EntityHeader user);


        Task<InvokeResult> UpdateLocationAsync(UpdateLocationViewModel location, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateOrganizationAsync(UpdateOrganizationViewModel orgViewModel, EntityHeader org, EntityHeader user);
    }
}
