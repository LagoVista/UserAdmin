using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;

namespace LagoVista.UserAdmin.Models.Orgs
{
    public class SubscriptionResource : SummaryData, INoSQLEntity
    {
        public string EntityType { get; set; }
        public string EntityTypeName { get; set; }
        public string DatabaseName { get; set; }
     
        public EntityHeader Subscription { get; set; }
    }
}
