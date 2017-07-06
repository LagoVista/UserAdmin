using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Interfaces
{
    /* Shoot me now....hack to make sign in manager testable */
    public interface ISignInManager
    {
        Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure);
    }
}
