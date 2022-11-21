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
