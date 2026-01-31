using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models.Resources;
using Microsoft.IdentityModel.Tokens;
using System;
using LagoVista.UserAdmin.Models;

[EntityDescription(
    Domains.AuthDomain,
    UserAdminResources.Names.TokenAuthOptions_Name,
    UserAdminResources.Names.TokenAuthOptions_Help,
    UserAdminResources.Names.TokenAuthOptions_Description,
    EntityDescriptionAttribute.EntityTypes.OrganizationModel,
    typeof(UserAdminResources))]
public class TokenAuthOptions
{
    public string Path { get; set; }
    public TimeSpan AccessExpiration { get; set; }
    public TimeSpan RefreshExpiration { get; set; }
    public string Audience { get; set; }
    public string Issuer { get; set; }
    public SigningCredentials SigningCredentials { get; set; }
}
