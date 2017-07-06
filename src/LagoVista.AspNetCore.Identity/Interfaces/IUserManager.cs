using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Interfaces
{
    public interface IUserManager
    {
        Task<AppUser> FindByNameAsync(string userName);
        Task<AppUser> FindByIdAsync(string id);
    }
}
