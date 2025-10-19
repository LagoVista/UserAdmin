// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 8ffc6f4da1aafd0079f4e4c34c854a59132b7ad8508034d6db26ca603bcd24bb
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Interfaces
{
    public interface IDefaultRoleList
    {
        IEnumerable<Role> GetStandardRoles();
    }
}
