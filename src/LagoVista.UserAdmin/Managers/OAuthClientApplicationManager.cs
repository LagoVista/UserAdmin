using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static LagoVista.Core.Models.AuthorizeResult;

namespace LagoVista.UserAdmin.Managers
{
    public class OAuthClientApplicationManager : ManagerBase, IOAuthClientApplicationManager
    {
        private readonly IOAuthClientApplicationRepo _repo;
        private readonly ISecureStorage _secureStorage;

        public OAuthClientApplicationManager(IOAuthClientApplicationRepo repo, ISecureStorage secureStorage, IDependencyManager depManager,
            ISecurity security, IAdminLogger logger, IAppConfig appConfig) : base(logger, appConfig, depManager, security)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _secureStorage = secureStorage ?? throw new ArgumentNullException(nameof(secureStorage));
        }

        public async Task<InvokeResult> AddOAuthClientApplicationAsync(OAuthClientApplication client, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(client, AuthorizeActions.Create, user, org);
            ValidationCheck(client, Actions.Create);

            if (await _repo.QueryClientIdInUseAsync(client.ClientId))
                return InvokeResult.FromError($"The OAuth client id [{client.ClientId}] is already registered.");

            if (!String.IsNullOrEmpty(client.ClientSecret))
            {
                var secretResult = await _secureStorage.AddSecretAsync(org, client.ClientSecret);
                if (!secretResult.Successful)
                    return secretResult.ToInvokeResult();

                client.ClientSecretId = secretResult.Result;
                client.ClientSecret = null;
            }

            await _repo.AddOAuthClientApplicationAsync(client);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateOAuthClientApplicationAsync(OAuthClientApplication client, EntityHeader org, EntityHeader user)
        {
            var existingClient = await _repo.GetOAuthClientApplicationAsync(client.Id);
            await AuthorizeAsync(existingClient, AuthorizeActions.Update, user, org);

            client.ClientSecretId = existingClient.ClientSecretId;
            ValidationCheck(client, Actions.Update);

            if (await _repo.QueryClientIdInUseAsync(client.ClientId, client.Id))
                return InvokeResult.FromError($"The OAuth client id [{client.ClientId}] is already registered.");

            var oldSecretId = existingClient.ClientSecretId;
            if (!String.IsNullOrEmpty(client.ClientSecret))
            {
                var secretResult = await _secureStorage.AddSecretAsync(org, client.ClientSecret);
                if (!secretResult.Successful)
                    return secretResult.ToInvokeResult();

                client.ClientSecretId = secretResult.Result;
                client.ClientSecret = null;
            }

            await _repo.UpdateOAuthClientApplicationAsync(client);

            if (!String.IsNullOrEmpty(oldSecretId) && oldSecretId != client.ClientSecretId)
                await _secureStorage.RemoveSecretAsync(org, oldSecretId);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> DeleteOAuthClientApplicationAsync(string id, EntityHeader org, EntityHeader user)
        {
            var client = await _repo.GetOAuthClientApplicationAsync(id);
            await AuthorizeAsync(client, AuthorizeActions.Delete, user, org);
            await ConfirmNoDepenenciesAsync(client);

            await _repo.DeleteOAuthClientApplicationAsync(id);

            if (!String.IsNullOrEmpty(client.ClientSecretId))
                await _secureStorage.RemoveSecretAsync(org, client.ClientSecretId);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckOAuthClientApplicationInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var client = await _repo.GetOAuthClientApplicationAsync(id);
            await AuthorizeAsync(client, AuthorizeActions.Read, user, org);
            return await CheckForDepenenciesAsync(client);
        }

        public async Task<OAuthClientApplication> GetOAuthClientApplicationAsync(string id, EntityHeader org, EntityHeader user)
        {
            var client = await _repo.GetOAuthClientApplicationAsync(id);
            await AuthorizeAsync(client, AuthorizeActions.Read, user, org);
            client.ClientSecret = null;
            return client;
        }

        public async Task<OAuthClientApplication> GetOAuthClientApplicationByClientIdAsync(string clientId)
        {
            var client = await _repo.GetOAuthClientApplicationByClientIdAsync(clientId);
            if (client != null)
                client.ClientSecret = null;
            return client;
        }

        public async Task<List<OAuthClientApplication>> GetOAuthClientApplicationsByPostLogoutRedirectUriAsync(string postLogoutRedirectUri)
        {
            var clients = await _repo.GetOAuthClientApplicationsByPostLogoutRedirectUriAsync(postLogoutRedirectUri);
            foreach (var client in clients)
                client.ClientSecret = null;
            return clients;
        }

        public async Task<ListResponse<OAuthClientApplicationSummary>> GetOAuthClientApplicationsForOrgAsync(EntityHeader org, EntityHeader user, ListRequest listRequest)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(OAuthClientApplication));
            return await _repo.GetOAuthClientApplicationsAsync(org.Id, listRequest);
        }

        public Task<bool> QueryKeyInUseAsync(string key, EntityHeader org)
        {
            return _repo.QueryKeyInUseAsync(key, org.Id);
        }

        public Task<bool> QueryClientIdInUseAsync(string clientId, string currentId = null)
        {
            return _repo.QueryClientIdInUseAsync(clientId, currentId);
        }
    }
}
