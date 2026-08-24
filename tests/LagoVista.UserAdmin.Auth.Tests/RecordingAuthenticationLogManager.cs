using LagoVista.Core;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    public sealed class RecordingAuthenticationLogManager : IAuthenticationLogManager
    {
        private readonly List<RecordedAuthenticationEvent> _events = new List<RecordedAuthenticationEvent>();

        public IReadOnlyList<RecordedAuthenticationEvent> Events => _events;

        public Task AddAsync(AuthenticationLog authLog)
        {
            if (authLog == null) throw new ArgumentNullException(nameof(authLog));

            _events.Add(new RecordedAuthenticationEvent
            {
                TypeName = authLog.AuthType,
                UserId = authLog.UserId,
                UserName = authLog.UserName,
                OrgId = authLog.OrganizationId,
                OrgName = authLog.Organization,
                Errors = authLog.Errors,
                Extras = authLog.Extras,
                InviteId = authLog.InviteId,
                RedirectUri = authLog.RedirectUri,
                OAuthProvider = authLog.OAuthProvider,
                ChallengeId = authLog.ChallengeId,
                CredentialId = authLog.CredentialId,
                AssertionId = authLog.AssertionId
            });

            return Task.CompletedTask;
        }

        public Task AddAsync(AuthLogTypes type, string userId = "?", string userName = "?", string orgId = "?", string orgName = "?", string oauthProvier = "", string errors = "", string extras = "", string redirectUri = "", string inviteId = "none", string credentialId = "", string challengeId = "", string assertionId = "")
        {
            _events.Add(new RecordedAuthenticationEvent
            {
                Type = type,
                TypeName = type.ToString(),
                UserId = userId,
                UserName = userName,
                OrgId = orgId,
                OrgName = orgName,
                OAuthProvider = oauthProvier,
                Errors = errors,
                Extras = extras,
                RedirectUri = redirectUri,
                InviteId = inviteId,
                CredentialId = credentialId,
                ChallengeId = challengeId,
                AssertionId = assertionId
            });

            return Task.CompletedTask;
        }

        public Task AddAsync(AuthLogTypes type, EntityHeader user = null, EntityHeader org = null, string oauthProvider = "", string errors = "", string extras = "", string redirectUri = "", string inviteId = "none", string credentialId = "", string challengeId = "", string assertionId = "")
        {
            return AddAsync(type,
                userId: user == null ? "?" : user.Id,
                userName: user == null ? "?" : user.Text,
                orgId: org == null ? "?" : org.Id,
                orgName: org == null ? "?" : org.Text,
                oauthProvier: oauthProvider,
                errors: errors,
                extras: extras,
                redirectUri: redirectUri,
                inviteId: inviteId,
                credentialId: credentialId,
                challengeId: challengeId,
                assertionId: assertionId);
        }

        public Task AddAsync(AuthLogTypes type, AppUser appUser, string oauthProvider = "", string errors = "", string extras = "", string redirectUri = "", string inviteId = "none", string credentialId = "", string challengeId = "", string assertionId = "")
        {
            if (appUser == null) throw new ArgumentNullException(nameof(appUser));

            return AddAsync(type,
                userId: appUser.Id,
                userName: appUser.UserName,
                orgId: appUser.CurrentOrganization == null ? Guid.Empty.ToId() : appUser.CurrentOrganization.Id,
                orgName: appUser.CurrentOrganization == null ? "?" : appUser.CurrentOrganization.Text,
                oauthProvier: oauthProvider,
                errors: errors,
                extras: extras,
                redirectUri: redirectUri,
                inviteId: inviteId,
                credentialId: credentialId,
                challengeId: challengeId,
                assertionId: assertionId);
        }

        public Task<ListResponse<AuthenticationLog>> GetAllAsync(ListRequest listRequest, EntityHeader org, EntityHeader user) => throw new NotSupportedException();
        public Task<ListResponse<AuthenticationLog>> GetAllAsync(string orgId, ListRequest listRequest, EntityHeader org, EntityHeader user) => throw new NotSupportedException();
        public Task<ListResponse<AuthenticationLog>> GetAsync(AuthLogTypes type, ListRequest listRequest, EntityHeader org, EntityHeader user) => throw new NotSupportedException();
        public Task<ListResponse<AuthenticationLog>> GetAsync(string orgId, AuthLogTypes type, ListRequest listRequest, EntityHeader org, EntityHeader user) => throw new NotSupportedException();
        public Task<ListResponse<AuthenticationLog>> GetForUserIdAsync(string userId, ListRequest listRequest, EntityHeader org, EntityHeader user) => throw new NotSupportedException();
        public Task<ListResponse<AuthenticationLog>> GetForUserNameAsync(string userName, ListRequest listRequest, EntityHeader org, EntityHeader user) => throw new NotSupportedException();
    }

    public sealed class RecordedAuthenticationEvent
    {
        public AuthLogTypes? Type { get; set; }
        public string TypeName { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string OrgId { get; set; }
        public string OrgName { get; set; }
        public string OAuthProvider { get; set; }
        public string Errors { get; set; }
        public string Extras { get; set; }
        public string RedirectUri { get; set; }
        public string InviteId { get; set; }
        public string ChallengeId { get; set; }
        public string CredentialId { get; set; }
        public string AssertionId { get; set; }
    }
}