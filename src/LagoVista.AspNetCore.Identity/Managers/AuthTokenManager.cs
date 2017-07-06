﻿using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.AspNetCore.Identity.Models;
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Models;
using LagoVista.Core;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Resources;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class AuthTokenManager : IAuthTokenManager
    {
        TokenAuthOptions _tokenOptions;
        IRefreshTokenManager _refreshTokenManager;
        IAdminLogger _adminLogger;
        IClaimsFactory _claimsFactory;
        SignInManager<AppUser> _signInManager;
        UserManager<AppUser> _userManager;

        private const string AUTH_TOKEN_TYPE = "auth";
        private const string GRANT_TYPE_PASSWORD = "password";
        private const string GRANT_TYPE_REFRESHTOKEN = "refreshtoken";

        private const string EMAIL_REGEX_FORMAT = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";

        public AuthTokenManager(TokenAuthOptions tokenOptions, IRefreshTokenManager refreshTokenManager, IAdminLogger adminLogger, IClaimsFactory claimsFactory, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _tokenOptions = tokenOptions;
            _refreshTokenManager = refreshTokenManager;
            _adminLogger = adminLogger;
            _signInManager = signInManager;
            _userManager = userManager;
            _claimsFactory = claimsFactory;
        }

        public async Task<InvokeResult<AuthResponse>> AuthAsync(AuthRequest authRequest, EntityHeader org = null, EntityHeader user = null)
        {
            if (String.IsNullOrEmpty(authRequest.AppId))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthMissingAppId.Message);
                return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthMissingAppId.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(authRequest.ClientId))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthMissingClientId.Message);
                return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthMissingClientId.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(authRequest.ClientType))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthMissingClientType.Message);
                return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthMissingClientType.ToErrorMessage());
            }

            if (authRequest.GrantType == GRANT_TYPE_PASSWORD)
            {
                if (String.IsNullOrEmpty(authRequest.Email))
                {
                    _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthMissingEmail.Message);
                    return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthMissingEmail.ToErrorMessage());
                }

                if (String.IsNullOrEmpty(authRequest.Password))
                {
                    _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthMissingPassword.Message);
                    return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthMissingPassword.ToErrorMessage());
                }

                var emailRegEx = new Regex(EMAIL_REGEX_FORMAT);
                if (!emailRegEx.Match(authRequest.Email).Success)
                {
                    _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthEmailInvalidFormat.Message, new KeyValuePair<string, string>("email", authRequest.Email));
                    return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthEmailInvalidFormat.ToErrorMessage());
                }

                var result = await _signInManager.PasswordSignInAsync(authRequest.UserName, authRequest.Password, true, false);
                if (result.Succeeded)
                {
                    var appUser = await _userManager.FindByNameAsync(authRequest.UserName);

                    if (appUser == null)
                    {
                        _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthCouldNotFindUserAccount.Message, new KeyValuePair<string, string>("email", authRequest.UserName));
                        return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthCouldNotFindUserAccount.ToErrorMessage());
                    }

                    var expires = DateTime.UtcNow.AddDays(_tokenOptions.AuthExpiration.TotalDays);
                    var token = GetJWToken(appUser, expires);
                    var refreshTokenResponse = await _refreshTokenManager.GenerateRefreshTokenAsync(authRequest.AppId, authRequest.ClientId, appUser.Id);
                    if (!refreshTokenResponse.Successful)
                    {
                        var failedResult = new InvokeResult<AuthResponse>();
                        failedResult.Concat(refreshTokenResponse);
                        return failedResult;
                    }

                    var authResponse = new AuthResponse()
                    {
                        AuthToken = token,
                        TokenType = AUTH_TOKEN_TYPE,
                        AuthTokenExpiresUTC = expires.ToJSONString(),
                        RefreshToken = refreshTokenResponse.Result.RowKey,
                        RefreshTokenExpiresUTC = refreshTokenResponse.Result.ExpiresUtc

                    };

                    return new InvokeResult<AuthResponse>() { Result = authResponse };
                }
                else
                {
                    return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthInvalidCredentials.ToErrorMessage());
                }
            }
            else if (authRequest.GrantType == GRANT_TYPE_REFRESHTOKEN)
            {
                if (user == null)
                {
                    _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthUserIsNullForRefresh.Message);
                    return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthUserIsNullForRefresh.ToErrorMessage());
                }

                if (String.IsNullOrEmpty(authRequest.RefreshToken))
                {
                    _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthMissingRefreshToken.Message);
                    return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthMissingRefreshToken.ToErrorMessage());
                }

                var appUser = await _userManager.FindByIdAsync(user.Id);
                if (appUser == null)
                {
                    _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthCouldNotFindUserAccount.Message, new KeyValuePair<string, string>("id", user.Id));
                    return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthCouldNotFindUserAccount.ToErrorMessage());
                }

                var expires = DateTime.UtcNow.AddDays(_tokenOptions.AuthExpiration.TotalDays);
                var offset = DateTimeOffset.Now;

                var token = GetJWToken(appUser, expires);
                var refreshTokenResponse = await _refreshTokenManager.RenewRefreshTokenAsync(authRequest.RefreshToken, user.Id);
                if (!refreshTokenResponse.Successful)
                {
                    var failedResult = new InvokeResult<AuthResponse>();
                    failedResult.Concat(refreshTokenResponse);
                    return failedResult;
                }

                var authResponse = new AuthResponse()
                {
                    AuthToken = token,
                    TokenType = AUTH_TOKEN_TYPE,
                    AuthTokenExpiresUTC = expires.ToJSONString(),
                    RefreshToken = refreshTokenResponse.Result.RowKey,
                    RefreshTokenExpiresUTC = refreshTokenResponse.Result.ExpiresUtc
                };

                return new InvokeResult<AuthResponse>() { Result = authResponse };
            }
            else
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthInvalidGrantType.Message);

                return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthInvalidGrantType.ToErrorMessage());
            }
        }

        private string GetJWToken(AppUser user, DateTime? expires)
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
                claims: _claimsFactory.GetClaims(user),
                notBefore: now,
                expires: now.Add(_tokenOptions.AuthExpiration),
                signingCredentials: _tokenOptions.SigningCredentials);
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        /* I guess nonce is similar to Guid but has more characeters and each character has more than 15 characters, guess it's really
         * just a very large randome string https://en.wikipedia.org/wiki/Cryptographic_nonce 
         * sorta going by a bit of trust here, but seems solid, feel free to add more notes or to cleaner implmenation. */
        public string NonceGenerator(string extra = "")
        {
            var result = String.Empty;
            var sha1 = SHA1.Create();
            var rand = new Random();

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
