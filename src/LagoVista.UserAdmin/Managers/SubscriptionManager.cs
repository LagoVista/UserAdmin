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
        readonly ISecureStorage _secureStorage;

        public SubscriptionManager(ISubscriptionRepo subscriptionRepo, ISecureStorage secureStorage, IDependencyManager depManager, IPaymentCustomers paymentCustomers, IAppUserRepo appUserRepo, IOrganizationRepo organizationRepo,
            ISubscriptionResourceRepo subscriptionResourceRepo, ISecurity security, IAdminLogger logger, IAppConfig appConfig) : base(logger, appConfig, depManager, security)
        {
            _subscriptionRepo = subscriptionRepo ?? throw new ArgumentNullException(nameof(subscriptionRepo));
            _paymentCustomers = paymentCustomers ?? throw new ArgumentNullException(nameof(paymentCustomers));
            _subscriptionResourceRepo = subscriptionResourceRepo ?? throw new ArgumentNullException(nameof(subscriptionResourceRepo));
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _organizationRepo = organizationRepo ?? throw new ArgumentNullException(nameof(organizationRepo));
            _secureStorage = secureStorage ?? throw new ArgumentNullException(nameof(secureStorage));
        }

        public async Task<InvokeResult> AddSubscriptionAsync(Subscription subscription, EntityHeader org, EntityHeader user)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));

            var timeStamp = UtcTimestamp.Now;
            if (EntityHeader.IsNullOrEmpty(subscription.OwnerOrganization)) subscription.OwnerOrganization = org;
            if (EntityHeader.IsNullOrEmpty(subscription.CreatedBy)) subscription.CreatedBy = user;
            if (EntityHeader.IsNullOrEmpty(subscription.LastUpdatedBy)) subscription.LastUpdatedBy = user;
            if (String.IsNullOrEmpty(subscription.CreationDate)) subscription.CreationDate = timeStamp;
            if (String.IsNullOrEmpty(subscription.LastUpdatedDate)) subscription.LastUpdatedDate = timeStamp;

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
            else if (subscription.Key == Subscription.SubscriptionKey_Provisional)
            {
                subscription.PaymentTokenStatus = Subscription.PaymentTokenStatus_Waived;
                subscription.Status = Subscription.Status_OK;
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
                
                    var secretId = await _secureStorage.AddSecretAsync(org, subscription.PaymentToken);
                    if(!secretId.Successful)  return secretId.ToInvokeResult();
                    subscription.PaymentTokenSecretId = secretId.Result;
                    subscription.PaymentToken = null;
                }
            }

            await _subscriptionRepo.AddSubscriptionAsync(subscription, org, user);

            return new InvokeResult();
        }

        public async Task<InvokeResult> EnsureProvisionalSubscriptionAsync(Subscription subscription, EntityHeader org, EntityHeader user)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            if (org == null || String.IsNullOrWhiteSpace(org.Id)) return InvokeResult.FromError("Organization is required.");
            if (user == null || String.IsNullOrWhiteSpace(user.Id)) return InvokeResult.FromError("User is required.");
            if (subscription.Key != Subscription.SubscriptionKey_Provisional) return InvokeResult.FromError("Only provisional subscriptions can use the provisional bootstrap path.");

            var existing = await _subscriptionRepo.GetSubscriptionAsync(subscription.Id, org, user);
            if (existing != null)
            {
                if (existing.Key != Subscription.SubscriptionKey_Provisional) return InvokeResult.FromError("The subscription ID is already in use by a non-provisional subscription.");
                if (existing.OwnerOrganization != null && existing.OwnerOrganization.Id != org.Id) return InvokeResult.FromError("The provisional subscription belongs to a different organization.");
                return await EnsureProvisionalDefaultSubscriptionAsync(existing, org);
            }

            var addResult = await AddSubscriptionAsync(subscription, org, user);
            if (!addResult.Successful) return addResult;

            return await EnsureProvisionalDefaultSubscriptionAsync(subscription, org);
        }

        private async Task<InvokeResult> EnsureProvisionalDefaultSubscriptionAsync(Subscription subscription, EntityHeader org)
        {
            var organization = await _organizationRepo.GetOrganizationAsync(org.Id);
            if (organization == null) return InvokeResult.FromError("The provisional organization could not be found.");

            var subscriptionId = subscription.Id.ToString();
            if (!EntityHeader.IsNullOrEmpty(organization.DefaultSubscription) &&
                String.Equals(organization.DefaultSubscription.Id, subscriptionId, StringComparison.OrdinalIgnoreCase))
            {
                return InvokeResult.Success;
            }

            organization.DefaultSubscription = EntityHeader.Create(subscriptionId, subscription.Name);
            await _organizationRepo.UpdateOrganizationAsync(organization);
            return InvokeResult.Success;
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

        public async Task<Subscription> GetSubscriptionAsync(GuidString36 id, EntityHeader org, EntityHeader user)
        {
            var subscription = await _subscriptionRepo.GetSubscriptionAsync(id, org, user);
            if (subscription == null) return null;

            await AuthorizeAsync(user, org, "getSubscription", subscription);
            if(subscription.PaymentTokenSecretId != null && subscription.PaymentTokenSecretId.StartsWith("src_"))
            {
                var secretId = await _secureStorage.AddSecretAsync(org, subscription.PaymentToken);
                if (!secretId.Successful)  throw new Exception("Unable to add payment token.");
                subscription.PaymentTokenSecretId = secretId.Result;
                subscription.PaymentToken = null;
                await UpdateSubscriptionAsync(subscription, org, user);
            }

            return subscription;
        }

        public async Task<ListResponse<SubscriptionResource>> GetResourcesForSubscriptionAsync(GuidString36 subscriptionId, ListRequest listRequest, EntityHeader org, EntityHeader user)
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

        public async Task<ListResponse<SubscriptionSummary>> GetSubscriptionsForCustomerAsync(GuidString36 customerId, ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(SubscriptionSummary));

            return await _subscriptionRepo.GetSubscriptionsForCustomerAsync(customerId, org, user, listRequest);
        }

        public async Task<InvokeResult> UpdateSubscriptionAsync(Subscription subscription, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(user, org, "updateSubscription", subscription);

            var oldSubscription = await _subscriptionRepo.GetSubscriptionAsync(subscription.Id, org, user);

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

                if(!String.IsNullOrEmpty(subscription.PaymentTokenSecretId))
                {
                    var deleteResult = await _secureStorage.RemoveSecretAsync(org, subscription.PaymentTokenSecretId);
                    if (!deleteResult.Successful) return deleteResult.ToInvokeResult();
                }

                var secretId = await _secureStorage.AddSecretAsync(org, subscription.PaymentToken);
                if(!secretId.Successful)  return secretId.ToInvokeResult();
                subscription.PaymentTokenSecretId = secretId.Result;
                subscription.PaymentToken = null;

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

        public async Task<InvokeResult> PurgeProvisionalSubscriptionAsync(string subscriptionId, string orgId, string appUserId)
        {
            if (await _organizationRepo.HasBillingRecords(orgId)) return InvokeResult.FromError("Organization has billing events and cannot be purged.");
            var validationResult = await ValidateProvisionalSubscriptionForPurgeAsync(subscriptionId, orgId, appUserId);
            if (!validationResult.Successful) return validationResult;

            var org = EntityHeader.Create(orgId, "Provisional Workspace");
            var user = EntityHeader.Create(appUserId, "Provisional Environment");
            var subscription = await _subscriptionRepo.GetSubscriptionAsync(subscriptionId, org, user);
            if (subscription != null) await _subscriptionRepo.DeleteSubscriptionAsync(subscriptionId, org, user);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> ValidateProvisionalSubscriptionForPurgeAsync(string subscriptionId, string orgId, string appUserId)
        {
            if (String.IsNullOrWhiteSpace(subscriptionId)) return InvokeResult.FromError("SubscriptionId is required.");
            if (String.IsNullOrWhiteSpace(orgId)) return InvokeResult.FromError("OrganizationId is required.");
            if (String.IsNullOrWhiteSpace(appUserId)) return InvokeResult.FromError("AppUserId is required.");
            var org = EntityHeader.Create(orgId, "Provisional Workspace");
            var user = EntityHeader.Create(appUserId, "Provisional Environment");
            var subscription = await _subscriptionRepo.GetSubscriptionAsync(subscriptionId, org, user);
            if (subscription == null) return InvokeResult.Success;
            if (subscription.Key != Subscription.SubscriptionKey_Provisional) return InvokeResult.FromError("The subscription is no longer provisional.");
            if (subscription.OwnerOrganization == null || subscription.OwnerOrganization.Id != orgId) return InvokeResult.FromError("The subscription does not belong to the provisional organization.");
            return InvokeResult.Success;
        }
    }
}
