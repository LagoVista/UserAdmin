using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.DTOs;

namespace LagoVista.UserAdmin.Models.Security
{
    public class RoleAccess
    {
        public string Id { get; set; }
        public bool IsPublic { get; set; }
        public EntityHeader Role { get; set; }
        public EntityHeader Organization { get; set; }
        public string CreationDate { get; set; }
        public EntityHeader CreatedBy { get; set; }

        public EntityHeader Module { get; set; }
        public EntityHeader Area { get; set; }
        public EntityHeader Page { get; set; }
        public EntityHeader Feature { get; set; }

        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
        public string Other { get; set; }


        public RoleAccessDTO ToDTO()
        {
            return new RoleAccessDTO()
            {
                RowKey = Id,
                PartitionKey = Organization.Id,

                RoleId = Role.Id,
                RoleKey = Role.Key,

                OrganizationId = Organization.Id,
                OrganizationName = Organization.Id,

                IsPublic = IsPublic,

                CreatedById = CreatedBy.Id,
                CreatedByName = CreatedBy.Text,

                Create = Create,
                Read = Read,
                Update = Update,
                Delete = Delete,
                Other = Other
            };
        }
    }
}
