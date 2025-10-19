// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 09db975f5ae6d96f51da9202fdf52351454a0fce2fd01e04663c88e9b52e47b0
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Users
{
    public class SingleUseToken
    {
        public string UserId { get; set; }
        public string Token { get; set; }
        public string Expires { get;  set; }
    }
}
