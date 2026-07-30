using LagoVista.Core;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Resources;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public class InvitationAcceptanceService : IInvitationAcceptanceService
    {
        private readonly IOrganizationManager _organizationManager;
        private readonly IInviteUserRepo _inviteUserRepo;
        private readonly IAppUserRepo _appUserRepo;
        private readonly IAuthenticationLogManager _authenticationLogManager;

        public InvitationAcceptanceService(IOrganizationManager organizationManager, IInviteUserRepo inviteUserRepo, IAppUserRepo appUserRepo, IAuthenticationLogManager authenticationLogManager)
        {
            _organizationManager = organizationManager ?? throw new ArgumentNullException(nameof(organizationManager));
            _inviteUserRepo = inviteUserRepo ?? throw new ArgumentNullException(nameof(inviteUserRepo));
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _authenticationLogManager = authenticationLogManager ?? throw new ArgumentNullException(nameof(authenticationLogManager));
        }

        public async Task<InvokeResult<AcceptInviteResponse>> AcceptInvitationAsync(string inviteId, string userId)
        {
            var acceptedUser = await _appUserRepo.FindByIdAsync(userId);
            if (acceptedUser == null)
                return InvokeResult<AcceptInviteResponse>.FromError($"Could not find user [{userId}] while accepting invitation [{inviteId}].");

            var invite = await _inviteUserRepo.GetInvitationAsync(inviteId);
            if (invite == null || !invite.IsActive())
            {
                var reason = invite == null ? "Could not load invite" : $"Status: {invite.Status}";
                await _authenticationLogManager.AddAsync(AuthLogTypes.InviteAcceptanceFailed, acceptedUser, inviteId: inviteId, extras: $"Accept not valid to be accepted, Status: {reason}.");
                var failedResult = InvokeResult<AcceptInviteResponse>.FromErrors(UserAdminErrorCodes.AuthInviteNotActive.ToErrorMessage());
                failedResult.RedirectURL = $"{CommonLinks.InviteAcceptedFailed}?err={failedResult.ErrorMessage}";
                return failedResult;
            }

            var invitingUser = EntityHeader.Create(invite.InvitedById, invite.InvitedByName);
            var organization = EntityHeader.Create(invite.OrganizationId, invite.OrganizationName);
            var currentOrganization = acceptedUser.CurrentOrganization;

            var membershipResult = await _organizationManager.AddUserToOrgAsync(acceptedUser, organization, invitingUser);
            if (!membershipResult.Successful)
            {
                await _authenticationLogManager.AddAsync(AuthLogTypes.InviteAcceptanceFailed, acceptedUser, inviteId: inviteId, extras: membershipResult.ErrorMessage);
                var failedResult = InvokeResult<AcceptInviteResponse>.FromInvokeResult(membershipResult);
                failedResult.RedirectURL = $"{CommonLinks.InviteAcceptedFailed}?err={membershipResult.ErrorMessage}";
                return failedResult;
            }

            acceptedUser.CurrentOrganization = currentOrganization;
            InvitationAcceptanceUserStateUpdater.ApplyAcceptedMembership(acceptedUser, organization);

            invite.Accepted = true;
            invite.Status = Invitation.StatusTypes.Accepted;
            invite.DateAccepted = DateTime.UtcNow.ToJSONString();

            await _inviteUserRepo.UpdateInvitationAsync(invite);
            await _appUserRepo.UpdateAsync(acceptedUser);

            await _authenticationLogManager.AddAsync(AuthLogTypes.InviteAcceptanceSucceeded, acceptedUser.ToEntityHeader(), currentOrganization?.ToEntityHeader() ?? organization, inviteId: inviteId);

            var response = new AcceptInviteResponse
            {
                RedirectPage = $"{CommonLinks.InviteAccepted}?inviteid={inviteId}&emailconfirmed={acceptedUser.EmailConfirmed.ToString().ToLower()}",
                ResponseMessage = $"Congratulations! You have accepted the invitation from {invite.InvitedByName} to the {invite.OrganizationName} organization. "
            };

            if (!String.IsNullOrEmpty(invite.EndUserAppOrg) && !String.IsNullOrEmpty(invite.EndUserAppOrgId))
                response.EndUserAppOrg = EntityHeader.Create(invite.EndUserAppOrgId, invite.EndUserAppOrg);

            if (!String.IsNullOrEmpty(invite.Customer) && !String.IsNullOrEmpty(invite.CustomerId))
                response.Customer = EntityHeader.Create(invite.CustomerId, invite.Customer);

            if (!String.IsNullOrEmpty(invite.CustomerContact) && !String.IsNullOrEmpty(invite.CustomerContactId))
                response.CustomerContact = EntityHeader.Create(invite.CustomerContactId, invite.CustomerContact);

            return InvokeResult<AcceptInviteResponse>.Create(response);
        }
    }
}
