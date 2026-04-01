using LagoVista;
using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.Core.Attributes;
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
        var accessExpires = section.Require("AccessTokenExpirationMinutes");
        var refreshExpires = section.Require("RefreshTokenExpirationDays");

        // If any of these required value are missing startup won't complete so we don't need to set these values.
        if (String.IsNullOrEmpty(accessExpires) || string.IsNullOrEmpty(refreshExpires) || string.IsNullOrEmpty(SecretKey))
        { 
            return;
        }

        if (int.TryParse(accessExpires, out var accessExpiresMinutes))
            AccessExpiration = TimeSpan.FromMinutes(accessExpiresMinutes);
        else
            AccessExpiration = TimeSpan.FromMinutes(15);

        if(int.TryParse(refreshExpires, out var refreshExpiresDays))
            RefreshExpiration = TimeSpan.FromDays(refreshExpiresDays);
        else
            RefreshExpiration = TimeSpan.FromDays(7);

        var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));
        SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
    }

    public string Path { get; set; }
  
    public TimeSpan AccessExpiration { get; set; }
    public TimeSpan RefreshExpiration { get; set; }
    public string Audience { get; set; }
    public string Issuer { get; set; }
    public SigningCredentials SigningCredentials { get; set; }
}
