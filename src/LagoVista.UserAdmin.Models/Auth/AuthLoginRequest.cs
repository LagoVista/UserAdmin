using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Auth
{
    public class AuthLoginRequest
    {
        public string EndUserAppOrgId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string InviteId { get; set; }
        public bool LockoutOnFailure { get; set; }
    }
}
