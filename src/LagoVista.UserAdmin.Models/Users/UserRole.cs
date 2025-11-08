// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ef5e14085db755d9e1b597abb9905cdac5cf996c7e83ab90685f7ff192d8ef39
// IndexVersion: 2
// --- END CODE INDEX META ---
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
        public string CreationDate { get; set; }
        public EntityHeader CreatedBy { get; set; }

        public UserRoleDTO ToDTO()
        {
            return new UserRoleDTO()
            {
                RowKey = Id,
                PartitionKey = Organization.Id,

                RoleId = Role.Id,
                RoleKey = Role.Key,
                RoleName = Role.Text,

                UserId = User.Id,
                UserName = User.Text,

                OrganizationId = Organization.Id,
                OrganizationName = Organization.Text,

                CreationDate = CreationDate,

                CreatedById = CreatedBy.Id,
                CreatedByName = CreatedBy.Text
            };
        }

        public EntityHeader ToEntityHeader()
        {
            return Role;
        }
    }
}
