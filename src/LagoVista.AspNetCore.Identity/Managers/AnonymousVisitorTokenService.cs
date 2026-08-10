using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.UserAdmin;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class AnonymousVisitorTokenService : IAnonymousVisitorTokenService
    {
        private readonly ITokenAuthOptions _tokenOptions;

        public AnonymousVisitorTokenService(ITokenAuthOptions tokenOptions)
        {
            _tokenOptions = tokenOptions ?? throw new ArgumentNullException(nameof(tokenOptions));
        }

        public string CreateToken(string actorId, DateTime accessExpiresUtc)
        {
            if (String.IsNullOrWhiteSpace(actorId)) throw new ArgumentNullException(nameof(actorId));

            var now = DateTime.UtcNow;
            var claims = new List<Claim>
            {
                new Claim(ClaimsFactory.Anonymous, Boolean.TrueString),
                new Claim(ClaimsFactory.ActorId, actorId),
                new Claim(ClaimsFactory.IdentityStage, ClaimsFactory.VisitorIdentityStage),
                new Claim(ClaimsFactory.IsPreviewUser, Boolean.FalseString),
                new Claim(ClaimsFactory.ExternalAccountVerified, Boolean.FalseString),
                new Claim(ClaimsFactory.EmailVerified, Boolean.FalseString),
                new Claim(ClaimsFactory.PhoneVerfiied, Boolean.FalseString),
                new Claim(ClaimsFactory.IsSystemAdmin, Boolean.FalseString),
                new Claim(ClaimsFactory.IsOrgAdmin, Boolean.FalseString),
                new Claim(ClaimsFactory.IsAppBuilder, Boolean.FalseString),
                new Claim(ClaimsFactory.IsUserDevice, Boolean.FalseString),
                new Claim(ClaimsFactory.IsCustomerAdmin, Boolean.FalseString),
                new Claim(ClaimsFactory.IsFinancceAdmin, Boolean.FalseString),
                new Claim(ClaimsFactory.TwoFactorEnabled, Boolean.FalseString),
                new Claim(ClaimsFactory.OrgRequireMfa, Boolean.FalseString),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var jwt = new JwtSecurityToken(
                issuer: _tokenOptions.Issuer,
                audience: _tokenOptions.Audience,
                claims: claims,
                notBefore: now,
                expires: accessExpiresUtc,
                signingCredentials: _tokenOptions.SigningCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
