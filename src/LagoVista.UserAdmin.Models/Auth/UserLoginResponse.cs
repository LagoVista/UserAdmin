using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Auth
{
    public class UserLoginResponse : EntityHeader
    {
        public AppUser User { get; set; }
    
        public UserFavorites Favorites { get; set; }
        public MostRecentlyUsed MostRecentlyUsed { get; set; }
    }
}
