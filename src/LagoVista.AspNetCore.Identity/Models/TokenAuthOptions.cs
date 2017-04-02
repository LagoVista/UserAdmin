using Microsoft.IdentityModel.Tokens;
using System;

namespace LagoVista.AspNetCore.Identity.Models
{
    public class TokenAuthOptions
    {
        public String Path { get; set; } 
        public TimeSpan Expiration { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public SigningCredentials SigningCredentials { get; set; }
    }
}
