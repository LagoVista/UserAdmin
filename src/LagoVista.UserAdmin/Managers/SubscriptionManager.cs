using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.PlatformSupport;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using System;
using System.Collections.Generic;
using System.Text;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using System.Threading.Tasks;
using LagoVista.Core.Authentication.Exceptions;

namespace LagoVista.UserAdmin.Managers
{
    public class SubscriptionManager : ManagerBase, ISubscriptionManager
    {
        ISubscriptionRepo _subscriptionRepo;
        public SubscriptionManager(ISubscriptionRepo subscriptionRepo, ILogger logger, IAppConfig appConfig) : base(logger, appConfig)
        {
            _subscriptionRepo = subscriptionRepo;
        }

        public async Task<InvokeResult> AddSubscriptionAsync(Subscription subscription, EntityHeader org, EntityHeader user)
        {
            var authResult = await AuthorizeAsync(subscription, AuthorizeResult.AuthorizeActions.Create, user, org);
            if(!authResult.IsAuthorized)
            {
                throw new NotAuthorizedException(authResult);
            }

            await _subscriptionRepo.AddSubscriptionAsync(subscription);

            return authResult.ToActionResult();
        }

        public Task<bool> CanDeleteSubscriptionAsync(Subscription subscription, EntityHeader org, EntityHeader user)
        {
            return Task.FromResult(true);
        }

        public async Task<InvokeResult> DeleteSubscriptionAsync(String id, EntityHeader org, EntityHeader user)
        {
            var subscription = await _subscriptionRepo.GetSubscriptionAsync(id);
            var authResult = await AuthorizeAsync(subscription, AuthorizeResult.AuthorizeActions.Delete, user, org);
            if (!authResult.IsAuthorized)
            {
                throw new NotAuthorizedException(authResult);
            }

            await _subscriptionRepo.DeleteSubscriptionAsync(subscription.Id);

            return authResult.ToActionResult();
        }

        public async Task<Subscription> GetSubscriptionAsync(string id, EntityHeader org, EntityHeader user)
        {
            var subscription = await _subscriptionRepo.GetSubscriptionAsync(id);
            var authResult = await AuthorizeAsync(subscription, AuthorizeResult.AuthorizeActions.Read, user, org);
            if (!authResult.IsAuthorized)
            {
                throw new NotAuthorizedException(authResult);
            }

            return subscription;            
        }

        public async Task<IEnumerable<SubscriptionSummary>> GetSubscriptionsForOrgAsync(string orgId, EntityHeader user)
        {
            var authResult = await AuthorizeOrgAccess(user, orgId, typeof(SubscriptionSummary));
            if (!authResult.IsAuthorized)
            {
                throw new NotAuthorizedException(authResult);
            }

            return await _subscriptionRepo.GetSubscriptionsForOrgAsync(orgId);
        }

        public Task<bool> QueryKeyInUse(string key, EntityHeader org)
        {
            return _subscriptionRepo.QueryKeyInUse(key, org.Id);
        }

        public async Task<InvokeResult> UpdateSubscriptionAsync(Subscription subscription, EntityHeader org, EntityHeader user)
        {
            var authResult = await AuthorizeAsync(subscription, AuthorizeResult.AuthorizeActions.Update, user, org);
            if (!authResult.IsAuthorized)
            {
                throw new NotAuthorizedException(authResult);
            }

            await _subscriptionRepo.UpdateSubscriptionAsync(subscription);

            return authResult.ToActionResult();
        }
    }
}
