using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Users
{
    public class UserRole
    {
        public string Id { get; set; }
        public EntityHeader User { get; set; }
        public EntityHeader Role { get; set; }
        public EntityHeader Organization { get; set; }
        public string CreeatedOn { get; set; }
        public EntityHeader CreatedBy { get; set; }

        public UserRoleDTO ToDTO()
        {
            return new UserRoleDTO()
            {
                RowKey = Id,
                PartitionKey = Organization.Id,

                RoleId = Role.Id,
                RoleKey = Role.Key,

                UserId = User.Id,
                UserName = User.Text,

                OrganizationId = Organization.Id,
                OrganizationName = Organization.Id,

                CreatedById = CreatedBy.Id,
                CreatedByName = CreatedBy.Text
            };
        }
    }
}
