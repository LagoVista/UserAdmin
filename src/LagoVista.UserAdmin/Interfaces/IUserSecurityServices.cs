using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces
{
    public interface IUserSecurityServices
    {
        Task<List<string>> GetRolesForUserAsync(string userId, string organization);
    }
}
