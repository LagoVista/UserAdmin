using LagoVista;
using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

[EntityDescription(
    Domains.AuthDomain,
    UserAdminResources.Names.TokenAuthOptions_Name,
    UserAdminResources.Names.TokenAuthOptions_Help,
    UserAdminResources.Names.TokenAuthOptions_Description,
    EntityDescriptionAttribute.EntityTypes.OrganizationModel,
    typeof(UserAdminResources))]
public class TokenAuthOptions : ITokenAuthOptions
{
    public TokenAuthOptions(IConfiguration config)
    {
        var section = config.GetSection("TokenAuth");
        Audience = section.Require("Audience");
        Issuer = section.Require("Issuer");
        var SecretKey = section.Require("SecretKey");
        var accessExpires = section.GetValue<int>("AccessTokenExpirationMinutes", 60);
        var refreshExpires = section.GetValue<int>("RefreshTokenExpirationDays", 7);

        var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));
        SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        AccessExpiration = TimeSpan.FromMinutes(accessExpires);
        RefreshExpiration = TimeSpan.FromMinutes(refreshExpires);
    }

    public string Path { get; set; }
  
    public TimeSpan AccessExpiration { get; set; }
    public TimeSpan RefreshExpiration { get; set; }
    public string Audience { get; set; }
    public string Issuer { get; set; }
    public SigningCredentials SigningCredentials { get; set; }
}
