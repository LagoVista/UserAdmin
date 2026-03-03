// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 25d6e22b8aabac707bd9f7fa07a829964c202af6fd23bb90c22ed30878118b51
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using System;
using System.Linq;
using System.Collections.Generic;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using System.Threading.Tasks;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Core.Exceptions;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.Core;

namespace LagoVista.UserAdmin.Managers
{
    public class SubscriptionManager : ManagerBase, ISubscriptionManager
    {
        readonly IPaymentCustomers _paymentCustomers;
        readonly ISubscriptionRepo _subscriptionRepo;
        readonly ISubscriptionResourceRepo _subscriptionResourceRepo;
        readonly IAppUserRepo _appUserRepo;
        readonly IOrganizationRepo _organizationRepo;

        public SubscriptionManager(ISubscriptionRepo subscriptionRepo, IDependencyManager depManager, IPaymentCustomers paymentCustomers, IAppUserRepo appUserRepo, IOrganizationRepo organizationRepo,
            ISubscriptionResourceRepo subscriptionResourceRepo, ISecurity security, IAdminLogger logger, IAppConfig appConfig) : base(logger, appConfig, depManager, security)
        {
            _subscriptionRepo = subscriptionRepo ?? throw new ArgumentNullException(nameof(subscriptionRepo));
            _paymentCustomers = paymentCustomers ?? throw new ArgumentNullException(nameof(paymentCustomers));
            _subscriptionResourceRepo = subscriptionResourceRepo ?? throw new ArgumentNullException(nameof(subscriptionResourceRepo));
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _organizationRepo = organizationRepo ?? throw new ArgumentNullException(nameof(organizationRepo));
        }

        public async Task<InvokeResult> AddSubscriptionAsync(Subscription subscription, EntityHeader org, EntityHeader user)
        {
            if (subscription.Key == Subscription.SubscriptionKey_Trial)
            {
                var subscriptions = await GetTrialSubscriptionAsync(org, user);
                if (subscriptions != null)
                {
                    throw new ValidationException("Invalid Data", new List<ErrorMessage>(){new ErrorMessage("Organization already has one trial subscription.")});
                }
                else
                {
                    subscription.Status = Subscription.Status_OK;
                    subscription.PaymentTokenStatus = Subscription.PaymentTokenStatus_Waived;
                }
            }
            else
            {
                if (String.IsNullOrEmpty(subscription.PaymentToken))
                {
                    subscription.PaymentTokenStatus = Subscription.PaymentTokenStatus_Empty;
                    subscription.Status = Subscription.Status_NoPaymentDetails;
                }
                else
                {
                    var result = await _paymentCustomers.CreateCustomerAsync(subscription.PaymentAccountId, subscription.PaymentToken);
                    if (!result.Successful) return result.ToInvokeResult();

                    subscription.PaymentAccountType = "stripe";
                    subscription.PaymentAccountId = result.Result;
                    subscription.PaymentTokenStatus = Subscription.PaymentTokenStatus_OK;
                    subscription.Status = Subscription.Status_OK;
                    subscription.PaymentTokenDate = CalendarDate.Today();
                }
            }

            await _subscriptionRepo.AddSubscriptionAsync(subscription, org, user);

            return new InvokeResult();
        }

        public async Task<Subscription> GetTrialSubscriptionAsync(EntityHeader org, EntityHeader user)
        {
            var subscription = await _subscriptionRepo.GetTrialSubscriptionAsync(org.Id, org, user);
            if (subscription != null)
            {
                await AuthorizeAsync(user, org, "getTrialSubscription", subscription);
            }

            return subscription;
        }

        public async Task<Subscription> GetSubscriptionAsync(Guid id, EntityHeader org, EntityHeader user)
        {
            var subscription = await _subscriptionRepo.GetSubscriptionAsync(id, org, user);
            await AuthorizeAsync(user, org, "getSubscription", subscription);
            return subscription;
        }

        public async Task<ListResponse<SubscriptionResource>> GetResourcesForSubscriptionAsync(Guid subscriptionId, ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            var subscription = await GetSubscriptionAsync(subscriptionId, org, user);
            await AuthorizeAsync(user, org, "getResourcesForSubscription", subscription);

            return await _subscriptionResourceRepo.GetResourcesForSubscriptionAsync(subscriptionId, listRequest, org.Id);
        }

        public async Task<ListResponse<SubscriptionSummary>> GetSubscriptionsForOrgAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(SubscriptionSummary));

            return await _subscriptionRepo.GetSubscriptionsForOrgAsync(org, user, listRequest);
        }

        public async Task<ListResponse<SubscriptionSummary>> GetSubscriptionsForCustomerAsync(Guid customerId, ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(SubscriptionSummary));

            return await _subscriptionRepo.GetSubscriptionsForCustomerAsync(customerId, org, user, listRequest);
        }

        public async Task<InvokeResult> UpdateSubscriptionAsync(Subscription subscription, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(user, org, "updateSubscription", subscription);

            var oldSubscription = await _subscriptionRepo.GetSubscriptionAsync(new Guid(subscription.Id), org, user);

            ValidationCheck(subscription, Actions.Update);

            if (oldSubscription.PaymentToken != subscription.PaymentToken)
            {
                if (String.IsNullOrEmpty(oldSubscription.PaymentToken))
                {
                    var result = await _paymentCustomers.CreateCustomerAsync(subscription.Id.ToString(), subscription.PaymentToken);
                    if (!result.Successful) return result.ToInvokeResult();
                    subscription.PaymentAccountId = result.Result;
                }
                else
                {
                    var result = await _paymentCustomers.AddPaymentSource(subscription.PaymentAccountId, subscription.PaymentToken);
                    if (!result.Successful) return result.ToInvokeResult();
                }

                subscription.PaymentTokenStatus = Subscription.PaymentTokenStatus_OK;
                subscription.Status = Subscription.Status_OK;
                subscription.PaymentTokenDate = CalendarDate.Today();
            }

            await _subscriptionRepo.UpdateSubscriptionAsync(subscription, org, user);

            return new InvokeResult();
        }

        public async Task<InvokeResult> DeleteSubscriptionsForOrgAsync(string orgId, EntityHeader org, EntityHeader user)
        {
            var hasBillingEvents = await _organizationRepo.HasBillingRecords(orgId);
            if (hasBillingEvents)
            {
                return InvokeResult.FromError("Organization has billing events, can not remove.");
            }

            var appUser = await _appUserRepo.FindByIdAsync(user.Id);
            var fullOrg = await _organizationRepo.GetOrganizationAsync(orgId);

            await AuthorizeAsync(user, org, "DeleteAllSubscriptions", fullOrg);

            await _subscriptionRepo.DeleteSubscriptionsForOrgAsync(org, user);

            return InvokeResult.Success;
        }
    }
}
