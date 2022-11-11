using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Users;

namespace LagoVista.UserAdmin.Models.DTOs
{
    public class UserRoleDTO : TableStorageEntity
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string RoleId { get; set; }
        public string RoleKey { get; set; }
        public string CreeatedOn { get; set; }
        public string CreatedById { get; set; }
        public string CreatedByName { get; set; }

        public UserRole ToUserRole()
        {
            return new UserRole()
            {
                Id = RowKey,
                User = EntityHeader.Create(UserId, UserName),
                Organization = EntityHeader.Create(OrganizationId, OrganizationName),
                Role = EntityHeader.Create(RoleId, RoleKey),
                CreatedBy = EntityHeader.Create(CreatedById, CreatedByName),
                CreeatedOn = CreeatedOn
            };
        }

    }
}
