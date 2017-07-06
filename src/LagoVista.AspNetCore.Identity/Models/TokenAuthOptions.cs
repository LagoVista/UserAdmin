using Microsoft.IdentityModel.Tokens;
using System;

namespace LagoVista.AspNetCore.Identity.Models
{
    public class TokenAuthOptions
    {
        public String Path { get; set; } 
        public TimeSpan AuthExpiration { get; set; }
        public TimeSpan RefreshExpiration { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public SigningCredentials SigningCredentials { get; set; }
    }
}
