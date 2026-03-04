using LagoVista.Core.Interfaces;
using LagoVista.Core.Interfaces.AutoMapper;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Relational;
using LagoVista.Relational.DataContexts;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.RDBMS
{
    public class SubscriptionRepo : RelationalBase<BillingDataContext>, ISubscriptionRepo
    {
        ILagoVistaAutoMapper _autoMapper;

        public SubscriptionRepo(BillingDataContext context, IAdminLogger adminLogger, ILagoVistaAutoMapper autoMapper, ISecureStorage secureStorage) :
           base(context, adminLogger, secureStorage)
        {
            _autoMapper = autoMapper ?? throw new ArgumentNullException(nameof(autoMapper));
        }

        public Task AddSubscriptionAsync(Subscription subscription, EntityHeader org, EntityHeader user)
        {
            return AddWithContextAsync(org, user, async ctx =>
            {
                var dto = await _autoMapper.CreateAsync<Subscription, SubscriptionDTO>(subscription, org, user);
                ctx.Subscription.Add(dto);
                await CommitAsync();
                ctx.ChangeTracker.Clear();
            });
        }

        public Task DeleteSubscriptionAsync(GuidString36 id, EntityHeader org, EntityHeader user)
        {
            return DeleteWithContextAsync(org, user, async ctx =>
            {
                await ctx.Subscription
                .ReadonlyQuery()
                .Where(usr => usr.Id == id.DbId)
                .ExecuteDeleteAsync();
                await CommitAsync();
                ctx.ChangeTracker.Clear();
            }); 
        }

        public Task DeleteSubscriptionsForOrgAsync(EntityHeader org, EntityHeader user)
        {
            return DeleteWithContextAsync(org, user, async ctx =>
            {
                await ctx.Subscription
                .ReadonlyQuery()
                .Where(usr => usr.OrganizationId == org.Id)
                .ExecuteDeleteAsync();
                await CommitAsync();
                ctx.ChangeTracker.Clear();
            });
        }

        public Task<Subscription> GetSubscriptionAsync(GuidString36 id, EntityHeader org, EntityHeader user)
        {
            return WithContextAsync(org, user, ctx =>
            {
                return  ctx.Subscription
                .ReadonlyQuery()
                .Include(usr => usr.Organization)
                .Include(usr => usr.CreatedByUser)
                .Include(usr => usr.LastUpdatedByUser)
                .Include(usr => usr.Customer)
                .Where(usr => usr.Id == id.DbId)
                .SingleMapAsync(async dto => await _autoMapper.CreateAsync<SubscriptionDTO, Subscription>(dto,org, user));
            });
        }

        public Task<ListResponse<SubscriptionSummary>> GetSubscriptionsForOrgAsync(EntityHeader org, EntityHeader user, ListRequest listRequest)
        {

            return WithContextAsync(org, user, ctx =>
            {
                return ctx.Subscription
                    .ReadonlyQuery()
                    .Where(inv => inv.OrganizationId == org.Id)
                    .Include(inv => inv.Customer)
                    .Include(inv => inv.Organization)
                    .ApplyKeysetPaging(listRequest, inv => inv.Name, inv => inv.Id)
                    .ToListResponseAsync(listRequest, async dto => await _autoMapper.CreateAsync<SubscriptionDTO, SubscriptionSummary>(dto, org, user), t => t.Name, t => t.Id.ToString());
            });
        }

        public Task<ListResponse<SubscriptionSummary>> GetSubscriptionsForCustomerAsync(GuidString36 customerId, EntityHeader org, EntityHeader user, ListRequest listRequest)
        {

            return WithContextAsync(org, user, ctx =>
            {
                return ctx.Subscription
                    .ReadonlyQuery()
                    .Where(inv => inv.OrganizationId == org.Id && inv.CustomerId == customerId.DbId)
                    .Include(inv => inv.Customer)
                    .Include(inv => inv.Organization)
                    .ApplyKeysetPaging(listRequest, inv => inv.Name, inv => inv.Id)
                    .ToListResponseAsync(listRequest, async dto => await _autoMapper.CreateAsync<SubscriptionDTO, SubscriptionSummary>(dto, org, user), t => t.Name, t => t.Id.ToString());
            });
        }

        public Task<Subscription> GetTrialSubscriptionAsync(string orgId, EntityHeader org, EntityHeader user)
        {
            return WithContextAsync(org, user, ctx =>
            {
                return ctx.Subscription
                .ReadonlyQuery()
                .Include(usr => usr.Organization)
                .Include(usr => usr.CreatedByUser)
                .Include(usr => usr.LastUpdatedByUser)
                .Include(usr => usr.Customer)
                .Where(usr => usr.OrganizationId == orgId && usr.Key == SubscriptionDTO.SubscriptionKey_Trial)
                .SingleMapAsync(async dto => await _autoMapper.CreateAsync<SubscriptionDTO, Subscription>(dto, org, user));
            });

            throw new NotImplementedException();
        }

        public Task UpdateSubscriptionAsync(Subscription subscription, EntityHeader org, EntityHeader user)
        {
            return UpdateWithContextAsync(org, user, async ctx =>
            {
                var dto = await _autoMapper.CreateAsync<Subscription, SubscriptionDTO>(subscription, org, user);
                ctx.Subscription.Update(dto);
                await CommitAsync();
                ctx.ChangeTracker.Clear();
            });
        }
    }
}
