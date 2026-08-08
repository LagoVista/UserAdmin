using LagoVista.Core.Interfaces;
using LagoVista.Relational;
using LagoVista.Relational.DataContexts;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Relational
{
    internal class ProvisionalEnvironmentArchiveAccountingService : IProvisionalEnvironmentArchiveAccountingService
    {
        private const string ArchiveSubscriptionKey = "archived-provisional-usage";
        private const string ArchiveSubscriptionName = "Archived Provisional Usage";
        private const string RollupIdNamespace = "provisional-environment-archive-rollup";

        private readonly IDbContextFactory<BillingDataContext> _contextFactory;
        private readonly IAppConfig _appConfig;
        private readonly ISystemUsers _systemUsers;

        public ProvisionalEnvironmentArchiveAccountingService(IDbContextFactory<BillingDataContext> contextFactory, IAppConfig appConfig, ISystemUsers systemUsers)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            _systemUsers = systemUsers ?? throw new ArgumentNullException(nameof(systemUsers));
        }

        public async Task<ProvisionalEnvironmentArchiveAccountingResult> EnsureRollupAsync(ProvisionalEnvironmentArchiveAccountingRequest request)
        {
            Validate(request);

            var billingEvents = request.BillingEvents.OrderBy(item => item.StartTimestamp).ThenBy(item => item.Id).ToList();
            var archiveSubscription = await EnsureArchiveSubscriptionAsync();
            var result = CreateResult(request, archiveSubscription.Subscription.Id, archiveSubscription.AlreadyExisted);
            if (billingEvents.Count == 0) return result;

            var rollupId = CreateDeterministicGuid($"{RollupIdNamespace}:{request.Environment.Id}");
            result.RollupBillingEventId = rollupId.ToString("D");
            result.ProductId = billingEvents[0].ProductId;

            await using var ctx = await _contextFactory.CreateDbContextAsync();
            var existing = await ctx.BillingEvents.ReadonlyQuery().SingleOrDefaultAsync(item => item.Id == rollupId);
            if (existing != null)
            {
                ValidateExistingRollup(existing, request, result);
                result.RollupAlreadyExisted = true;
                return result;
            }

            var first = billingEvents[0];
            if (!Guid.TryParse(first.ProductId, out var productId)) throw new InvalidOperationException("The archived billing event product ID is invalid.");

            var startUtc = billingEvents.Min(item => item.StartTimestamp).ToUniversalTime();
            var endUtc = billingEvents.Max(item => item.EndTimestamp ?? item.StartTimestamp).ToUniversalTime();
            var rollup = new BillingEventDTO
            {
                Id = rollupId,
                SubscriptionId = archiveSubscription.Subscription.Id,
                ProductId = productId,
                StartTimestamp = startUtc,
                StartedByAppUserId = _systemUsers.HostUser.Id,
                EndTimestamp = endUtc,
                EndedByAppUserId = _systemUsers.HostUser.Id,
                BillingDate = DateOnly.FromDateTime(startUtc),
                IdempotencyKey = $"provisional-archive:{request.Environment.Id}",
                BillingTimeZoneId = first.BillingTimeZoneId,
                Status = "Completed",
                HoursBilled = billingEvents.Sum(item => item.HoursBilled ?? 0m),
                Tokens = result.TotalTokens,
                UnitPrice = result.TotalExtended,
                UnitCost = result.TotalActualCost,
                ActualCost = result.TotalActualCost,
                UnitTypeId = first.UnitTypeId,
                Extended = result.TotalExtended,
                Quantity = 1m,
                ResourceId = CreateResourceId(request.Environment.Id),
                ResourceName = ArchiveSubscriptionName,
                Notes = CreateNotes(request, result),
                RollupType = BillingEventRollupTypes.Monthly
            };

            ctx.BillingEvents.Add(rollup);
            try
            {
                await ctx.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ctx.ChangeTracker.Clear();
                existing = await ctx.BillingEvents.ReadonlyQuery().SingleOrDefaultAsync(item => item.Id == rollupId);
                if (existing == null) throw;
                ValidateExistingRollup(existing, request, result);
                result.RollupAlreadyExisted = true;
            }

            return result;
        }

        private async Task<(SubscriptionDTO Subscription, bool AlreadyExisted)> EnsureArchiveSubscriptionAsync()
        {
            if (_appConfig.SystemOwnerOrg == null || String.IsNullOrWhiteSpace(_appConfig.SystemOwnerOrg.Id)) throw new InvalidOperationException("SystemOwnerOrg is not configured.");
            if (_systemUsers.HostUser == null || String.IsNullOrWhiteSpace(_systemUsers.HostUser.Id)) throw new InvalidOperationException("HostUser is not configured.");

            await using var ctx = await _contextFactory.CreateDbContextAsync();
            var existing = await ctx.Subscription.ReadonlyQuery().SingleOrDefaultAsync(item => item.OrganizationId == _appConfig.SystemOwnerOrg.Id && item.Key == ArchiveSubscriptionKey);
            if (existing != null) return (existing, true);

            var now = DateTime.UtcNow;
            var subscription = new SubscriptionDTO
            {
                Id = CreateDeterministicGuid($"{ArchiveSubscriptionKey}:{_appConfig.SystemOwnerOrg.Id}"),
                OrganizationId = _appConfig.SystemOwnerOrg.Id,
                CreatedById = _systemUsers.HostUser.Id,
                LastUpdatedById = _systemUsers.HostUser.Id,
                CreationDate = now,
                LastUpdatedDate = now,
                Key = ArchiveSubscriptionKey,
                Name = ArchiveSubscriptionName,
                Description = "Permanent accounting control totals for archived provisional environments.",
                Icon = "icon-ae-bill-1",
                Start = DateOnly.FromDateTime(now),
                ActiveDate = DateOnly.FromDateTime(now),
                IsActive = true,
                IsTrial = false,
                PaymentTokenStatus = Subscription.PaymentTokenStatus_Waived,
                Status = Subscription.Status_OK,
                PaymentAccountType = "system"
            };

            ctx.Subscription.Add(subscription);
            try
            {
                await ctx.SaveChangesAsync();
                return (subscription, false);
            }
            catch (DbUpdateException)
            {
                ctx.ChangeTracker.Clear();
                existing = await ctx.Subscription.ReadonlyQuery().SingleOrDefaultAsync(item => item.OrganizationId == _appConfig.SystemOwnerOrg.Id && item.Key == ArchiveSubscriptionKey);
                if (existing == null) throw;
                return (existing, true);
            }
        }

        private static ProvisionalEnvironmentArchiveAccountingResult CreateResult(ProvisionalEnvironmentArchiveAccountingRequest request, Guid archiveSubscriptionId, bool archiveSubscriptionAlreadyExisted)
        {
            return new ProvisionalEnvironmentArchiveAccountingResult
            {
                ArchiveSubscriptionId = archiveSubscriptionId.ToString("D"),
                BillingEventCount = request.BillingEvents.Count,
                TotalActualCost = request.BillingEvents.Sum(item => item.ActualCost ?? 0m),
                TotalExtended = request.BillingEvents.Sum(item => item.Extended ?? 0m),
                TotalTokens = request.BillingEvents.Sum(item => item.Tokens ?? 0L),
                TotalQuantity = request.BillingEvents.Sum(item => item.Quantity ?? 0m),
                ArchiveSubscriptionAlreadyExisted = archiveSubscriptionAlreadyExisted
            };
        }

        private static void Validate(ProvisionalEnvironmentArchiveAccountingRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Environment == null) throw new ArgumentNullException(nameof(request.Environment));
            if (request.Archive == null) throw new ArgumentNullException(nameof(request.Archive));
            if (request.BillingEvents == null) throw new ArgumentNullException(nameof(request.BillingEvents));
            if (String.IsNullOrWhiteSpace(request.Environment.Id)) throw new InvalidOperationException("The provisional environment ID is required.");
            if (String.IsNullOrWhiteSpace(request.Archive.ArchivePath)) throw new InvalidOperationException("A verified archive path is required before creating an accounting rollup.");
            if (String.IsNullOrWhiteSpace(request.Archive.BillingEventsSha256)) throw new InvalidOperationException("A verified billing-event archive hash is required before creating an accounting rollup.");
            if (request.Archive.BillingEventCount != request.BillingEvents.Count) throw new InvalidOperationException("The verified archive billing-event count does not match the accounting input.");
            if (request.BillingEvents.Any(item => !String.Equals(item.SubscriptionId, request.Environment.SubscriptionId, StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException("The accounting input contains a billing event from another subscription.");
        }

        private static void ValidateExistingRollup(BillingEventDTO existing, ProvisionalEnvironmentArchiveAccountingRequest request, ProvisionalEnvironmentArchiveAccountingResult expected)
        {
            if (existing.SubscriptionId.ToString("D") != expected.ArchiveSubscriptionId || existing.ActualCost != expected.TotalActualCost || existing.Extended != expected.TotalExtended || existing.Tokens != expected.TotalTokens || String.IsNullOrWhiteSpace(existing.Notes) || !existing.Notes.Contains(request.Archive.BillingEventsSha256, StringComparison.Ordinal))
                throw new InvalidOperationException($"The existing archival billing rollup for provisional environment '{request.Environment.Id}' does not match the verified archive.");
        }

        private static string CreateNotes(ProvisionalEnvironmentArchiveAccountingRequest request, ProvisionalEnvironmentArchiveAccountingResult result)
        {
            return JsonSerializer.Serialize(new Dictionary<string, object>
            {
                ["kind"] = "provisional-environment-archive-control-total",
                ["provisionalEnvironmentId"] = request.Environment.Id,
                ["originalAppUserId"] = request.Environment.AppUserId,
                ["originalOrganizationId"] = request.Environment.OrganizationId,
                ["originalSubscriptionId"] = request.Environment.SubscriptionId,
                ["archivePath"] = request.Archive.ArchivePath,
                ["billingEventsSha256"] = request.Archive.BillingEventsSha256,
                ["billingEventCount"] = result.BillingEventCount,
                ["totalQuantity"] = result.TotalQuantity
            });
        }

        private static Guid CreateDeterministicGuid(string value)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
            var bytes = new byte[16];
            Array.Copy(hash, bytes, bytes.Length);
            return new Guid(bytes);
        }

        private static string CreateResourceId(string provisionalEnvironmentId)
        {
            if (provisionalEnvironmentId.Length <= 32) return provisionalEnvironmentId;
            return CreateDeterministicGuid(provisionalEnvironmentId).ToString("N");
        }
    }
}
