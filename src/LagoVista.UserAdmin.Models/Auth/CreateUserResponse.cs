// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 5e840bdd4de0c8ac51b13ed2244ba71239553a3d75b1cde7d715f083d2e35dae
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Text;
using LagoVista.UserAdmin.Models.Users;

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

        public UserSetupStates UserSetupState { get; set; } = UserSetupStates.Unknown;

        public string ResponseMessage { get; set; }

        public string RedirectPage { get; set; }
    }
}
