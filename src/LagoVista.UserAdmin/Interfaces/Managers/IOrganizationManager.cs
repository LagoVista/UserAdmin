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
        Task<InvokeResult> AddAccountToOrgAsync(String orgId, String userId, EntityHeader addedBy);
        Task<InvokeResult> CreateOrganizationAsync(Organization newOrg, EntityHeader userOrg, EntityHeader user);
        Task<InvokeResult> UpdateOrganizationAsync(Organization org, EntityHeader userOrg, EntityHeader user);
        
        Task<InvokeResult> CreateLocationAsync(OrganizationLocation location, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateLocationAsync(OrganizationLocation location, EntityHeader org, EntityHeader user);


        Task<InvokeResult> AcceptInvitationAsync(AcceptInviteViewModel acceptInviteViewModel, string acceptedUserId);
        Task<LocationAccountRoles> AddAccountRoleForLocationAsync(EntityHeader location, EntityHeader account, EntityHeader role, EntityHeader addedBy);
        Task<OrganizationAccountRoles> AddAccountRoleForOrgAsync(EntityHeader location, EntityHeader account, EntityHeader role, EntityHeader addedBy);
        Task<InvokeResult> AddAccountToOrgAsync(EntityHeader userToAdd, EntityHeader org, EntityHeader addedBy = null);
        Task<InvokeResult> AddAccountToLocationAsync(string accountId, string locationId, EntityHeader addedBy = null);
        Task<InvokeResult> AddLocationAsync(CreateLocationViewModel newLocation, EntityHeader addedByUser);
        Task<InvokeResult> CreateNewOrganizationAsync(CreateOrganizationViewModel organizationViewModel, EntityHeader user);

        Task DeclineInvitationAsync(String inviteId);
        CreateLocationViewModel GetCreateLocationViewModel(EntityHeader org, EntityHeader user);
        Task<IEnumerable<LocationAccountRoles>> GetAccountRolesForLocationAsync(string locationId, string accountId);
        Task<IEnumerable<OrganizationAccountRoles>> GetAccountRolesForOrgAsync(string orgId, string accountId);
        Task GetAccountsForLocationAsync(string locationId);
        Task<IEnumerable<OrganizationAccount>> GetAccountsForOrganizationsAsync(string orgId);
        Task<IEnumerable<LocationAccountRoles>> GetAccountsForRoleInLocationAsync(string locationId, string roleId);
        Task<IEnumerable<OrganizationAccountRoles>> GetAccountsForRoleInOrgAsync(string orgId, string roleId);
        Task<AcceptInviteViewModel> GetInviteViewModelAsync(string inviteId);
        Task<IEnumerable<OrganizationLocation>> GetLocationsForOrganizationsAsync(String orgId);
        Task<IEnumerable<LocationAccount>> GetLocationsForAccountAsync(string accountId);
        Task<Organization> GetOrganizationAsync(string organizationId);
        Task<IEnumerable<OrganizationAccount>> GetOrganizationsForAccountAsync(string accountId);
        Task<UpdateLocationViewModel> GetUpdateLocationViewModelAsync(string locationId);
        Task<UpdateOrganizationViewModel> GetUpdateOrganizationViewModel(string organizationId);
        Task<Invitation> InviteUserAsync(InviteUserViewModel inviteViewModel, EntityHeader orgEntityHeader, EntityHeader userEntityHeader);

        Task<bool> QueryLocationNamespaceInUseAsync(string orgId, string namespaceText);
        Task<bool> QueryOrganizationHasAccountAsync(string orgId, string accountId);
        Task<bool> QueryOrgNamespaceInUseAsync(string namespaceText);

        Task<InvokeResult> RemoveAccountFromLocation(string locationId, string accountId, EntityHeader removedBy);
        Task<InvokeResult> RemoveAccountFromOrganizationAsync(string orgId, string accountId, EntityHeader removedBy);


        Task RevokeAllRolesForAccountInLocationAsync(EntityHeader location, EntityHeader account, EntityHeader revokedBy);
        Task RevokeAllRolesForAccountInOrgAsync(EntityHeader org, EntityHeader account, EntityHeader revokedBy);
        Task RevokeRoleForAccountInLocationAsync(EntityHeader location, EntityHeader role, EntityHeader account, EntityHeader revokedBy);
        Task RevokeRoleForAccountInOrgAsync(EntityHeader org, EntityHeader role, EntityHeader account, EntityHeader revokedBy);
        Task RevokeInvitationAsync(String inviteId);


        Task<InvokeResult> UpdateLocationAsync(UpdateLocationViewModel location, EntityHeader user);
        Task<InvokeResult> UpdateOrganizationAsync(UpdateOrganizationViewModel orgViewModel, EntityHeader user);
    }
}
