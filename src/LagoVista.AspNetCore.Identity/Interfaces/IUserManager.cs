using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Interfaces
{
    /* Hack ot make UserManager<AppUser> testable :/ */
    public interface IUserManager
    {
        Task<AppUser> FindByNameAsync(string userName);
        Task<AppUser> FindByIdAsync(string id);
        Task<IdentityResult> UpdateAsync(AppUser appUser);
    }
}
