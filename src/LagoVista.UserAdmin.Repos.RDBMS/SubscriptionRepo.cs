using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using System;
using System.Collections.Generic;
using System.Linq;
using LagoVista.UserAdmin.Models.Orgs;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LagoVista.Core.Exceptions;
using LagoVista.Core.Models;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace LagoVista.UserAdmin.Repos.RDBMS
{
    public class SubscriptionRepo : ISubscriptionRepo
    {
        private readonly IRDBMSConnectionProvider _connectionProvider;
        UserAdminDataContext _dataCtx;
        public SubscriptionRepo(UserAdminDataContext dataContext, IRDBMSConnectionProvider connectionProvider)
        {
            _dataCtx = dataContext;
            _connectionProvider = connectionProvider;
        }


        public async Task<UserAdminDataContext> GetBillingDataContextAsync(string orgId)
        {
            var connection = await _connectionProvider.GetConnectionAsync(orgId);
            switch (connection.ConnectionType)
            {
                case RDBMSConnectionType.PosgreSQLDedicated:

                    var options = new DbContextOptionsBuilder<UserAdminDataContext>().UseNpgsql(connection.ConnectionString);
                    if (connection.VerboseLogging)
                    {
                        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                        var logger = loggerFactory.CreateLogger<RDBMSManager>();

                        options = options.UseLoggerFactory(loggerFactory);
                    }

                    return new UserAdminDataContext(options.Options);
                default:
                    return _dataCtx;
            }
        }


        public async Task AddSubscriptionAsync(SubscriptionDTO subscription)
        {
            var ctx = await GetBillingDataContextAsync(subscription.OrgId);
            ctx.Subscription.Add(subscription);
            await ctx.SaveChangesAsync();
        }

        public async Task DeleteSubscriptionAsync(string orgId, Guid id)
        {
            var ctx = await GetBillingDataContextAsync(orgId);
            var subscription = await GetSubscriptionAsync(orgId, id);
            ctx.Subscription.Remove(subscription);
            await ctx.SaveChangesAsync();
        }

        public async Task<SubscriptionDTO> GetSubscriptionAsync(string orgId, Guid id, bool disableTracking = false)
        {
            var ctx = await GetBillingDataContextAsync(orgId);
            SubscriptionDTO subscription;
            if (disableTracking)
            {
                subscription = await ctx.Subscription.AsNoTracking().Where(prd => prd.Id == id).FirstOrDefaultAsync();
            }
            else
            {
                subscription = await ctx.Subscription.Where(prd => prd.Id == id).FirstOrDefaultAsync();
            }

            if (subscription == null)
            {
                throw new RecordNotFoundException("Subscription", id.ToString());
            }

            return subscription;
        }

        public async Task<SubscriptionDTO> GetTrialSubscriptionAsync(string orgId)
        {
            var ctx = await GetBillingDataContextAsync(orgId);
            return await ctx.Subscription.Where(prd => prd.OrgId == orgId && prd.Key == SubscriptionDTO.SubscriptionKey_Trial).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<SubscriptionSummary>> GetSubscriptionsForOrgAsync(string orgId)
        {
            var ctx = await GetBillingDataContextAsync(orgId);
            var subscriptions = await ctx.Subscription.Where(pc => pc.OrgId == orgId).ToListAsync();
            return from sub in subscriptions select sub.CreateSummary();
        }

        public async Task<bool> QueryKeyInUse(string key, string orgId)
        {
            var ctx = await GetBillingDataContextAsync(orgId);
            return await ctx.Subscription.Where(pc => pc.OrgId == orgId && pc.Key == key).AnyAsync();
        }


        public async Task UpdateSubscriptionAsync(SubscriptionDTO subscription)
        {
            // Need to ensure this gets inserted at DatTime type UTC
            subscription.CreationDate = new DateTime(subscription.CreationDate.Ticks, DateTimeKind.Utc);
            subscription.LastUpdatedDate = DateTime.UtcNow;

            var ctx = await GetBillingDataContextAsync(subscription.OrgId);
            ctx.Subscription.Update(subscription);
            await  ctx.SaveChangesAsync();
        }

        public async Task DeleteSubscriptionsForOrgAsync(string orgId)
        {
            var ctx = await GetBillingDataContextAsync(orgId);
            var subscriptions = await ctx.Subscription.Where(pc => pc.OrgId == orgId).ToListAsync();
            foreach (var subscription in subscriptions)
            {
                ctx.Subscription.Remove(subscription);
            }

            await ctx.SaveChangesAsync();
        }
    }
}
