using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Users;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Auth
{

    public class AuthResponse
    {
        public AuthResponse()
        {
            Roles = new List<EntityHeader>();
        }

        public string AccessToken { get; set; }

        public string AccessTokenExpiresUTC { get; set; }

        public string RefreshToken { get; set; }

        public string RefreshTokenExpiresUTC { get; set; }

        public string AppInstanceId { get; set; }

        public bool IsLockedOut { get; set; }

        public AppUser AppUser { get; set; }

        public EntityHeader User { get; set; }
        public EntityHeader Org { get; set; }
        public List<EntityHeader> Roles { get; set; }
    }
}
