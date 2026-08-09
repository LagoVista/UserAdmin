using LagoVista.Core.Interfaces;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Relational;
using LagoVista.Relational.DataContexts;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Relational
{
    internal class ProvisionalEnvironmentBillingArchiveRepo : RelationalBase<BillingDataContext>, IProvisionalEnvironmentBillingArchiveRepo
    {
        public ProvisionalEnvironmentBillingArchiveRepo(IDbContextFactory<BillingDataContext> context, IAdminLogger adminLogger, ISecureStorage secureStorage) : base(context, adminLogger, secureStorage)
        {
        }

        public async Task<IReadOnlyCollection<ProvisionalEnvironmentBillingEventArchive>> GetBillingEventsAsync(string organizationId, string subscriptionId)
        {
            if (String.IsNullOrWhiteSpace(organizationId)) throw new ArgumentNullException(nameof(organizationId));
            if (!Guid.TryParse(subscriptionId, out var parsedSubscriptionId)) throw new ArgumentException("SubscriptionId must be a GUID.", nameof(subscriptionId));

            await using var ctx = CreateContext();
            var billingEvents = await ctx.BillingEvents
                .ReadonlyQuery()
                .Where(billingEvent => billingEvent.SubscriptionId == parsedSubscriptionId && billingEvent.Subscription.OrganizationId == organizationId)
                .OrderBy(billingEvent => billingEvent.StartTimestamp)
                .ThenBy(billingEvent => billingEvent.Id)
                .ToListAsync();

            return billingEvents.Select(ToArchive).ToList();
        }

        public async Task<int> DeleteBillingEventsAsync(string organizationId, string subscriptionId, IReadOnlyCollection<string> billingEventIds)
        {
            if (String.IsNullOrWhiteSpace(organizationId)) throw new ArgumentNullException(nameof(organizationId));
            if (!Guid.TryParse(subscriptionId, out var parsedSubscriptionId)) throw new ArgumentException("SubscriptionId must be a GUID.", nameof(subscriptionId));
            if (billingEventIds == null) throw new ArgumentNullException(nameof(billingEventIds));
            if (billingEventIds.Count == 0) return 0;

            var parsedBillingEventIds = billingEventIds.Select(id => Guid.TryParse(id, out var parsedId) ? parsedId : throw new ArgumentException($"Billing event ID '{id}' is not a GUID.", nameof(billingEventIds))).ToList();
            await using var ctx = CreateContext();
            var currentIds = await ctx.BillingEvents
                .ReadonlyQuery()
                .Where(billingEvent => billingEvent.SubscriptionId == parsedSubscriptionId && billingEvent.Subscription.OrganizationId == organizationId)
                .Select(billingEvent => billingEvent.Id)
                .ToListAsync();

            if (currentIds.Count != parsedBillingEventIds.Count || currentIds.Except(parsedBillingEventIds).Any()) throw new InvalidOperationException("The provisional billing events changed after the archive was verified.");

            return await ctx.BillingEvents.Where(billingEvent => parsedBillingEventIds.Contains(billingEvent.Id)).ExecuteDeleteAsync();
        }

        private static ProvisionalEnvironmentBillingEventArchive ToArchive(BillingEventDTO billingEvent)
        {
            return new ProvisionalEnvironmentBillingEventArchive
            {
                Id = billingEvent.Id.ToString("D"),
                SubscriptionId = billingEvent.SubscriptionId.ToString("D"),
                ProductId = billingEvent.ProductId.ToString("D"),
                ModelUsageRateId = billingEvent.ModelUsageRateId?.ToString("D"),
                StartTimestamp = billingEvent.StartTimestamp,
                StartedByAppUserId = billingEvent.StartedByAppUserId,
                EndTimestamp = billingEvent.EndTimestamp,
                EndedByAppUserId = billingEvent.EndedByAppUserId,
                BillingDate = billingEvent.BillingDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                RolloverAt = billingEvent.RolloverAt,
                IdempotencyKey = billingEvent.IdempotencyKey,
                BillingTimeZoneId = billingEvent.BillingTimeZoneId,
                Status = billingEvent.Status,
                HoursBilled = billingEvent.HoursBilled,
                Tokens = billingEvent.Tokens,
                UnitPrice = billingEvent.UnitPrice,
                UnitCost = billingEvent.UnitCost,
                ActualCost = billingEvent.ActualCost,
                UnitTypeId = billingEvent.UnitTypeId,
                DiscountPercent = billingEvent.DiscountPercent,
                Extended = billingEvent.Extended,
                VendorUsageKey = billingEvent.VendorUsageKey,
                Quantity = billingEvent.Quantity,
                ResourceId = billingEvent.ResourceId,
                ResourceName = billingEvent.ResourceName,
                Notes = billingEvent.Notes,
                RollupType = billingEvent.RollupType
            };
        }
    }
}
