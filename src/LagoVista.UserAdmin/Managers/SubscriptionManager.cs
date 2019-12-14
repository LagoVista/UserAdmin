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

namespace LagoVista.UserAdmin.Managers
{
    public class SubscriptionManager : ManagerBase, ISubscriptionManager
    {
        IPaymentCustomers _paymentCustomers;
        ISubscriptionRepo _subscriptionRepo;
        public SubscriptionManager(ISubscriptionRepo subscriptionRepo, IDependencyManager depManager, IPaymentCustomers paymentCustomers, ISecurity security, IAdminLogger logger, IAppConfig appConfig) : base(logger, appConfig, depManager, security)
        {
            _subscriptionRepo = subscriptionRepo;
            _paymentCustomers = paymentCustomers;
        }

        public async Task<InvokeResult> AddSubscriptionAsync(Subscription subscription, EntityHeader org, EntityHeader user)
        {

            if (subscription.Key == Subscription.SubscriptionKey_Trial)
            {
                var subscriptions = await GetSubscriptionsForOrgAsync(org.Id, user);
                if (subscriptions.Where(sub => sub.Key == Subscription.SubscriptionKey_Trial).Any())
                {
                    throw new ValidationException("Invalid Data", new List<ErrorMessage>()
                                    {
                                        new ErrorMessage("Organization already has one trial subscription.")
                                    });
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
                    var result = await _paymentCustomers.CreateCustomerAsync(subscription.Id.ToString(), subscription.PaymentToken);
                    if (!result.Successful) return result.ToInvokeResult();

                    subscription.CustomerId = result.Result;
                    subscription.PaymentTokenStatus = Subscription.PaymentTokenStatus_OK;
                    subscription.Status = Subscription.Status_OK;
                    subscription.PaymentTokenDate = DateTime.UtcNow;
                }
            }

            await _subscriptionRepo.AddSubscriptionAsync(subscription);

            return new InvokeResult();
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(Guid id, EntityHeader org, EntityHeader user)
        {
            var subscription = await _subscriptionRepo.GetSubscriptionAsync(id);
            await AuthorizeAsync(user, org, "getSubscription", subscription);

            return await CheckForDepenenciesAsync(subscription);
        }

        public async Task<InvokeResult> DeleteSubscriptionAsync(Guid id, EntityHeader org, EntityHeader user)
        {
            var subscription = await _subscriptionRepo.GetSubscriptionAsync(id);
            await AuthorizeAsync(user, org, "deleteSubscription", subscription);
            await ConfirmNoDepenenciesAsync(subscription);

            await _subscriptionRepo.DeleteSubscriptionAsync(subscription.Id);

            return new InvokeResult();
        }

        public async Task<Subscription> GetTrialSubscriptionAsync(EntityHeader org, EntityHeader user)
        {
            var subscription = await _subscriptionRepo.GetTrialSubscriptionAsync(org.Id);
            await AuthorizeAsync(user, org, "getSubscription", subscription);

            return subscription;
        }


        public async Task<Subscription> GetSubscriptionAsync(Guid id, EntityHeader org, EntityHeader user)
        {
            var subscription = await _subscriptionRepo.GetSubscriptionAsync(id);
            await AuthorizeAsync(user, org, "getSubscription", subscription);

            return subscription;
        }

        public async Task<IEnumerable<SubscriptionSummary>> GetSubscriptionsForOrgAsync(string orgId, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, orgId, typeof(SubscriptionSummary));

            return await _subscriptionRepo.GetSubscriptionsForOrgAsync(orgId);
        }

        public Task<bool> QueryKeyInUseAsync(string key, EntityHeader org)
        {
            return _subscriptionRepo.QueryKeyInUse(key, org.Id);
        }

        public async Task<InvokeResult> UpdateSubscriptionAsync(Subscription subscription, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(user, org, "updateSubscription", subscription);

            var oldSubscription = await _subscriptionRepo.GetSubscriptionAsync(subscription.Id, true);

            ValidationCheck(subscription, Actions.Update);

            if (oldSubscription.PaymentToken != subscription.PaymentToken)
            {
                if (String.IsNullOrEmpty(oldSubscription.PaymentToken))
                {
                    var result = await _paymentCustomers.CreateCustomerAsync(subscription.Id.ToString(), subscription.PaymentToken);
                    if (!result.Successful) return result.ToInvokeResult();
                    subscription.CustomerId = result.Result;
                }
                else
                {
                    var result = await _paymentCustomers.AddPaymentSource(subscription.CustomerId, subscription.PaymentToken);
                    if (!result.Successful) return result.ToInvokeResult();
                }

                subscription.PaymentTokenStatus = Subscription.PaymentTokenStatus_OK;
                subscription.Status = Subscription.Status_OK;
                subscription.PaymentTokenDate = DateTime.UtcNow;
            }

            await _subscriptionRepo.UpdateSubscriptionAsync(subscription);

            return new InvokeResult();
        }
    }
}
