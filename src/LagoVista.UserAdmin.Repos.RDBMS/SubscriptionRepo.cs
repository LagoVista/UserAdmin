using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using System;
using System.Collections.Generic;
using System.Linq;
using LagoVista.UserAdmin.Models.Orgs;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LagoVista.Core.Exceptions;

namespace LagoVista.UserAdmin.Repos.RDBMS
{
    public class SubscriptionRepo : ISubscriptionRepo
    {
        UserAdminDataContext _dataContext;
        public SubscriptionRepo(UserAdminDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task AddSubscriptionAsync(Subscription subscription)
        {
            _dataContext.Subscription.Add(subscription);
            await _dataContext.SaveChangesAsync();
        }

        public async Task DeleteSubscriptionAsync(Guid id)
        {
            var subscription = await GetSubscriptionAsync(id);
            _dataContext.Subscription.Remove(subscription);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<Subscription> GetSubscriptionAsync(Guid id)
        {
            var subscription = await _dataContext.Subscription.Where(prd => prd.Id == id).FirstOrDefaultAsync();
            if (subscription == null)
            {
                throw new RecordNotFoundException("Product", id.ToString());
            }

            return subscription;
        }

        public async Task<IEnumerable<SubscriptionSummary>> GetSubscriptionsForOrgAsync(string orgId)
        {
            var subscriptions = await  _dataContext.Subscription.Where(pc => pc.OrgId == orgId).ToListAsync();
            return from sub in subscriptions select sub.CreateSummary();
        }

        public Task<bool> QueryKeyInUse(string key, string orgId)
        {
            return _dataContext.Subscription.Where(pc => pc.OrgId == orgId && pc.Key == key).AnyAsync();
        }

        public Task UpdateSubscriptionAsync(Subscription subscription)
        {
            _dataContext.Subscription.Update(subscription);
            return _dataContext.SaveChangesAsync();
        }
    }
}
