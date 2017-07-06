using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class UserManager : IUserManager
    {
        UserManager<AppUser> _userManager;
        public UserManager(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public Task<AppUser> FindByIdAsync(string id)
        {
            return _userManager.FindByIdAsync(id);
        }

        public Task<AppUser> FindByNameAsync(string userName)
        {
            return _userManager.FindByNameAsync(userName);
        }
    }
}
