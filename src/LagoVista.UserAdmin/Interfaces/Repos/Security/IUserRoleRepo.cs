﻿using LagoVista.UserAdmin.Models.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IUserRoleRepo
    {
        Task AddUserRole(UserRole role);
        Task RemoveUserRole(string userRoleId, string organizationId);
        Task<UserRole> GetRoleAssignmentAsync(string userRoleId, string organizationId);
        Task<List<UserRole>> GetRolesForUserAsync(string userId, string organizationId);
    }
}
