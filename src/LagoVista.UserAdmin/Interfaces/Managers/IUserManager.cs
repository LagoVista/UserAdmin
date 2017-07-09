using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    /* Hack ot make UserManager<AppUser> testable :/ */
    public interface IUserManager
    {
        Task<AppUser> FindByNameAsync(string userName);
        Task<AppUser> FindByIdAsync(string id);
        Task<InvokeResult> UpdateAsync(AppUser appUser);

        Task<InvokeResult> CreateAsync(AppUser user, string password);
    }
}
