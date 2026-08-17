using System;

namespace LagoVista.UserAdmin.Models.Orgs
{
    public static class SystemSubscriptionLevels
    {
        public static readonly Guid ProvisionalId = Guid.Parse("8C9B10D3-7F6A-4E5B-A11B-78D871D0704F");

        public static SubscriptionLevel CreateProvisional()
        {
            return new SubscriptionLevel
            {
                Id = ProvisionalId,
                Key = Subscription.SubscriptionKey_Provisional,
                Name = "Provisional",
                Description = "Default subscription level for provisional working environments",
                ProductId = null,
                IncludedWorkUnits = 100m,
                WorkUnitResetCycleTypeId = null,
                AllowsOverage = false,
                IsActive = true
            };
        }
    }
}
