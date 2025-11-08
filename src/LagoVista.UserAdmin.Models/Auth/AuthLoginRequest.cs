// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0801dff43253a58b717ac5e188be4edf4fcf975574fcd9b3555cd06794f4e57a
// IndexVersion: 2
// --- END CODE INDEX META ---
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
