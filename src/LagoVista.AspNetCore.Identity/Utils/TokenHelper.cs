using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.AspNetCore.Identity.Models;
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Users;
using System;
using LagoVista.Core;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace LagoVista.AspNetCore.Identity.Utils
{
    public class TokenHelper : ITokenHelper
    {
        TokenAuthOptions _tokenOptions;
        IClaimsFactory _claimsFactory;
        IAdminLogger _adminLogger;

        public TokenHelper(TokenAuthOptions tokenOptions, IClaimsFactory claimsFactory, IAdminLogger adminLogger)
        {
            _tokenOptions = tokenOptions;
            _claimsFactory = claimsFactory;
            _adminLogger = adminLogger;            
        }

        public InvokeResult<AuthResponse> GenerateAuthResponse(AppUser appUser, AuthRequest authRequest, InvokeResult<RefreshToken> refreshTokenResponse)
        {
            if (!refreshTokenResponse.Successful)
            {
                var failedResult = new InvokeResult<AuthResponse>();
                failedResult.Concat(refreshTokenResponse);
                return failedResult;
            }

            var accessExpires = DateTime.UtcNow.AddMinutes(_tokenOptions.AccessExpiration.TotalMinutes);
            var token = GetJWToken(appUser, accessExpires, authRequest.AppInstanceId);

            var authResponse = new AuthResponse()
            {
                AccessToken = token,
                AppInstanceId = authRequest.AppInstanceId,
                AccessTokenExpiresUTC = accessExpires.ToJSONString(),
                RefreshToken = refreshTokenResponse.Result.RowKey,
                Roles = appUser.CurrentOrganizationRoles,
                RefreshTokenExpiresUTC = refreshTokenResponse.Result.ExpiresUtc
            };

            if(!String.IsNullOrEmpty(authRequest.OrgId))
            {
                authResponse.Org = new Core.Models.EntityHeader() { Id = authRequest.OrgId, Text = authRequest.OrgName };
            }

            return InvokeResult<AuthResponse>.Create(authResponse);
        }

        public string GetJWToken(AppUser user, DateTime accessExpires, string installationId)
        {
            var handler = new JwtSecurityTokenHandler();

            var now = DateTime.UtcNow;

            var claims = _claimsFactory.GetClaims(user);
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, NonceGenerator()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            // Create the JWT and write it to a string
            var jwt = new JwtSecurityToken(
                issuer: _tokenOptions.Issuer,
                audience: _tokenOptions.Audience,
                claims: claims,
                notBefore: now,
                expires: accessExpires,
                signingCredentials: _tokenOptions.SigningCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public string NonceGenerator()
        {
            var result = String.Empty;
            var sha1 = SHA1.Create();
            var rand = new Random();
            var extra = String.Empty;

            while (result.Length < 32)
            {
                var generatedRandoms = new string[4];

                for (var i = 0; i < 4; i++)
                {
                    generatedRandoms[i] = rand.Next().ToString();
                }

                result += Convert.ToBase64String(sha1.ComputeHash(Encoding.ASCII.GetBytes(string.Join("", generatedRandoms) + "|" + extra))).Replace("=", "").Replace("/", "").Replace("+", "");
            }

            return result.Substring(0, 32);
        }
    }
}
