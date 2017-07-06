using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class SignInManager : ISignInManager
    {
        SignInManager<AppUser> _signinManager;
        public SignInManager(SignInManager<AppUser> signInManager)
        {
            _signinManager = signInManager;
        }

        public Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            return _signinManager.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
        }
    }
}
