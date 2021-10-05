using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using Newtonsoft.Json;

namespace LagoVista.UserAdmin.Models.Orgs
{
    public class OwnedObject : INamedEntity, IOwnedEntity, IIDEntity, IAuditableEntity, INoSQLEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string CreationDate { get; set; }
        public string LastUpdatedDate { get; set; }
        public EntityHeader CreatedBy { get; set; }
        public EntityHeader LastUpdatedBy { get; set; }
        public bool IsPublic { get; set; }
        public EntityHeader OwnerOrganization { get; set; }
        public EntityHeader OwnerUser { get; set; }
        public string Name { get; set; }
        public string DatabaseName { get; set; }
        public string EntityType { get; set; }
    }
}
