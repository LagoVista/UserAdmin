using System;

namespace LagoVista.UserAdmin.Models.Orgs
{
    public class SubscriptionLevel
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? ProductId { get; set; }
        public decimal IncludedWorkUnits { get; set; }
        public int? WorkUnitResetCycleTypeId { get; set; }
        public bool AllowsOverage { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
