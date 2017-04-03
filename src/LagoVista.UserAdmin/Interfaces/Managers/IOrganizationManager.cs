using LagoVista.Core.Models;
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
        Task AcceptInvitationAsync(AcceptInviteViewModel acceptInviteViewModel, string acceptedUserId);
        Task<LocationAccountRoles> AddAccountRoleForLocationAsync(EntityHeader location, EntityHeader account, EntityHeader role, EntityHeader addedBy);
        Task<OrganizationAccountRoles> AddAccountRoleForOrgAsync(EntityHeader location, EntityHeader account, EntityHeader role, EntityHeader addedBy);
        Task AddAccountToOrgAsync(EntityHeader userToAdd, EntityHeader org, EntityHeader addedBy = null);
        Task AddAccountTosync(string accountId, string locationId, EntityHeader addedBy = null);
        Task AddLocationAsync(CreateLocationViewModel newLocation, EntityHeader addedByUser);
        Task CreateNewOrganizationAsync(CreateOrganizationViewModel organizationViewModel, EntityHeader user);

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

        Task RemoveAccountFromLocation(string locationId, string accountId, EntityHeader removedBy);
        Task RemoveAccountFromOrganizationAsync(EntityHeader account, EntityHeader org, EntityHeader removedBy);
        Task RevokeAllRolesForAccountInLocationAsync(EntityHeader location, EntityHeader account, EntityHeader revokedBy);
        Task RevokeAllRolesForAccountInOrgAsync(EntityHeader org, EntityHeader account, EntityHeader revokedBy);
        Task RevokeRoleForAccountInLocationAsync(EntityHeader location, EntityHeader role, EntityHeader account, EntityHeader revokedBy);
        Task RevokeRoleForAccountInOrgAsync(EntityHeader org, EntityHeader role, EntityHeader account, EntityHeader revokedBy);
        Task RevokeInvitationAsync(String inviteId);
        Task UpdateLocationAsync(UpdateLocationViewModel location, EntityHeader user);
        Task UpdateOrganizationAsync(UpdateOrganizationViewModel orgViewModel, EntityHeader user);
    }
}
