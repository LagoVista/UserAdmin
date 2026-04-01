using Microsoft.IdentityModel.Tokens;
using System;

namespace LagoVista.AspNetCore.Identity.Interfaces
{
    public interface ITokenAuthOptions
    {
        public string Path { get; }
        public string Audience { get; }
        public string Issuer { get; }
        public TimeSpan AccessExpiration { get; }
        public TimeSpan RefreshExpiration { get; }
        SigningCredentials SigningCredentials { get; }
    }
}
