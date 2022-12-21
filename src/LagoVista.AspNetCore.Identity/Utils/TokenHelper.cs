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
using LagoVista.UserAdmin;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;

namespace LagoVista.AspNetCore.Identity.Utils
{
    public class TokenHelper : ITokenHelper
    {
        private readonly TokenAuthOptions _tokenOptions;
        private readonly IClaimsFactory _claimsFactory;
        private readonly IAdminLogger _adminLogger;
        private readonly IOrgUserRepo _orgUserRepo;
        private readonly IOrganizationManager _orgManager;
        private readonly IUserRoleRepo _userRoleRepo;

        public TokenHelper(TokenAuthOptions tokenOptions, IUserRoleRepo userRoleRepo, IOrganizationManager orgManager, IOrgUserRepo orgUserRepo, IClaimsFactory claimsFactory, IAdminLogger adminLogger)
        {
            _tokenOptions = tokenOptions ?? throw new ArgumentNullException(nameof(userRoleRepo));
            _claimsFactory = claimsFactory ?? throw new ArgumentNullException(nameof(userRoleRepo));
            _adminLogger = adminLogger ?? throw new ArgumentNullException(nameof(userRoleRepo));
            _orgManager = orgManager ?? throw new ArgumentNullException(nameof(userRoleRepo));
            _orgUserRepo = orgUserRepo ?? throw new ArgumentNullException(nameof(userRoleRepo));
            _userRoleRepo = userRoleRepo ?? throw new ArgumentNullException(nameof(userRoleRepo));
        }

        public async Task<InvokeResult<AuthResponse>> GenerateAuthResponseAsync(AppUser appUser, AuthRequest authRequest, InvokeResult<RefreshToken> refreshTokenResponse)
        {
            if (!refreshTokenResponse.Successful)
            {
                return InvokeResult<AuthResponse>.FromInvokeResult(refreshTokenResponse.ToInvokeResult());
            }

            var accessExpires = DateTime.UtcNow.AddMinutes(_tokenOptions.AccessExpiration.TotalMinutes);

            var authResponse = new AuthResponse()
            {
                AppInstanceId = authRequest.AppInstanceId,
                AccessTokenExpiresUTC = accessExpires.ToJSONString(),
                RefreshToken = refreshTokenResponse.Result.RowKey,
                RefreshTokenExpiresUTC = refreshTokenResponse.Result.ExpiresUtc,
                User = appUser.ToEntityHeader(),
            };

            if (!String.IsNullOrEmpty(authRequest.OrgId))
            {
                // we have already confirmed that the user has access to this org.
                authResponse.Org = new Core.Models.EntityHeader() { Id = authRequest.OrgId, Text = authRequest.OrgName };
                var orgRoles = await _userRoleRepo.GetRolesForUseAsync(appUser.Id, authRequest.OrgId);
                authResponse.Roles = new List<EntityHeader>(orgRoles.Select(rle => rle.ToEntityHeader()));
                var isOrgAdmin = await _orgUserRepo.IsUserOrgAdminAsync(authRequest.OrgId, authResponse.User.Id);
                var isAppBuilder = await _orgUserRepo.IsAppBuilderAsync(authRequest.OrgId, authResponse.User.Id);
                authResponse.AccessToken = GetJWToken(appUser, authResponse.Org, isOrgAdmin, isAppBuilder, accessExpires, authRequest.AppInstanceId);
            }
            else
            {
                authResponse.Roles = appUser.CurrentOrganizationRoles;
                authResponse.Org = appUser.CurrentOrganization;
                authResponse.AccessToken = GetJWToken(appUser, accessExpires, authRequest.AppInstanceId);
            }

            return InvokeResult<AuthResponse>.Create(authResponse);
        }

        public Task<InvokeResult<AuthResponse>> GenerateAuthResponseAsync(AppUser appUser, string appInstanceId, InvokeResult<RefreshToken> refreshTokenResponse)
        {
            if (!refreshTokenResponse.Successful)
            {
                return Task.FromResult(InvokeResult<AuthResponse>.FromInvokeResult(refreshTokenResponse.ToInvokeResult()));
            }

            var accessExpires = DateTime.UtcNow.AddMinutes(_tokenOptions.AccessExpiration.TotalMinutes);

            var authResponse = new AuthResponse()
            {
                AppInstanceId = appInstanceId,
                AccessTokenExpiresUTC = accessExpires.ToJSONString(),
                RefreshToken = refreshTokenResponse.Result.RowKey,
                RefreshTokenExpiresUTC = refreshTokenResponse.Result.ExpiresUtc,
                User = appUser.ToEntityHeader(),
            };

            authResponse.Roles = appUser.CurrentOrganizationRoles;
            authResponse.Org = appUser.CurrentOrganization;
            authResponse.AccessToken = GetJWToken(appUser, accessExpires, appInstanceId);

            return Task.FromResult(InvokeResult<AuthResponse>.Create(authResponse));
        }

        public string GetJWToken(AppUser user, DateTime accessExpires, string installationId)
        {
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

        public string GetJWToken(AppUser user, EntityHeader org, bool isOrgAdmin, bool isAppBuilder, DateTime accessExpires, string installationId)
        {
            var now = DateTime.UtcNow;
            var claims = _claimsFactory.GetClaims(user, org, isOrgAdmin, isAppBuilder);
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
