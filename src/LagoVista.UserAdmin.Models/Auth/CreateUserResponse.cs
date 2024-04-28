using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Auth
{
    public class CreateUserResponse : AuthResponse
    {
        public static CreateUserResponse FromAuthResponse(AuthResponse response)
        {
            return new CreateUserResponse()
            {
                User = response.User,
                AppUser = response.AppUser,
                AccessToken = response.AccessToken,
                AppInstanceId = response.AppInstanceId,
                AccessTokenExpiresUTC = response.AccessTokenExpiresUTC,
                Org = response.Org,
                IsLockedOut = response.IsLockedOut,
                RefreshToken = response.RefreshToken,
                RefreshTokenExpiresUTC = response.RefreshTokenExpiresUTC,
            };
        }

        public string ResponseMessage { get; set; }

        public string RedirectPage { get; set; }

    }
}
