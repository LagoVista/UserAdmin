// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7fc7317425cea7a021b98c8343f4bfaac3ce2a8486e661d79d1716f3e77366e6
// IndexVersion: 0
// --- END CODE INDEX META ---
using Microsoft.IdentityModel.Tokens;
using System;

namespace LagoVista.AspNetCore.Identity.Models
{
    public class TokenAuthOptions
    {
        public String Path { get; set; } 
        public TimeSpan AccessExpiration { get; set; }
        public TimeSpan RefreshExpiration { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public SigningCredentials SigningCredentials { get; set; }
    }
}
