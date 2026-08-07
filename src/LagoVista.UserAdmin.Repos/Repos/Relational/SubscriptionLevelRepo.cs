using LagoVista.Core.Interfaces;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Relational;
using LagoVista.Relational.DataContexts;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Relational
{
    public class SubscriptionLevelRepo : RelationalBase<BillingDataContext>, ISubscriptionLevelRepo
    {
        public SubscriptionLevelRepo(IDbContextFactory<BillingDataContext> context, IAdminLogger adminLogger, ISecureStorage secureStorage) : base(context, adminLogger, secureStorage)
        {
        }

        public async Task AddSubscriptionLevelAsync(SubscriptionLevel subscriptionLevel)
        {
            await using var ctx = CreateContext();
            ctx.SubscriptionLevels.Add(ToDto(subscriptionLevel));
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
        }

        public async Task UpdateSubscriptionLevelAsync(SubscriptionLevel subscriptionLevel)
        {
            await using var ctx = CreateContext();
            ctx.SubscriptionLevels.Update(ToDto(subscriptionLevel));
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
        }

        public async Task DeleteSubscriptionLevelAsync(Guid id)
        {
            await using var ctx = CreateContext();
            await ctx.SubscriptionLevels.Where(level => level.Id == id).ExecuteDeleteAsync();
            ctx.ChangeTracker.Clear();
        }

        public async Task<SubscriptionLevel> GetSubscriptionLevelAsync(Guid id)
        {
            await using var ctx = CreateContext();
            var dto = await ctx.SubscriptionLevels.ReadonlyQuery().SingleOrDefaultAsync(level => level.Id == id);
            return dto == null ? null : FromDto(dto);
        }

        public async Task<SubscriptionLevel> GetSubscriptionLevelByKeyAsync(string key)
        {
            await using var ctx = CreateContext();
            var dto = await ctx.SubscriptionLevels.ReadonlyQuery().SingleOrDefaultAsync(level => level.Key == key);
            return dto == null ? null : FromDto(dto);
        }

        public async Task<List<SubscriptionLevel>> GetSubscriptionLevelsAsync(bool activeOnly = false)
        {
            await using var ctx = CreateContext();
            var query = ctx.SubscriptionLevels.ReadonlyQuery();
            if (activeOnly)
                query = query.Where(level => level.IsActive);

            var items = await query.OrderBy(level => level.Name).ToListAsync();
            return items.Select(FromDto).ToList();
        }

        private static SubscriptionLevelDTO ToDto(SubscriptionLevel level)
        {
            return new SubscriptionLevelDTO
            {
                Id = level.Id,
                Key = level.Key,
                Name = level.Name,
                Description = level.Description,
                ProductId = level.ProductId,
                IncludedWorkUnits = level.IncludedWorkUnits,
                WorkUnitResetCycleTypeId = level.WorkUnitResetCycleTypeId,
                AllowsOverage = level.AllowsOverage,
                IsActive = level.IsActive
            };
        }

        private static SubscriptionLevel FromDto(SubscriptionLevelDTO dto)
        {
            return new SubscriptionLevel
            {
                Id = dto.Id,
                Key = dto.Key,
                Name = dto.Name,
                Description = dto.Description,
                ProductId = dto.ProductId,
                IncludedWorkUnits = dto.IncludedWorkUnits,
                WorkUnitResetCycleTypeId = dto.WorkUnitResetCycleTypeId,
                AllowsOverage = dto.AllowsOverage,
                IsActive = dto.IsActive
            };
        }
    }
}
