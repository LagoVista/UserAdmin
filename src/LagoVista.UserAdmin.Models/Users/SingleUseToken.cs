using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Users
{
    public class SingleUseToken
    {
        public string UserId { get; set; }
        public string Token { get; set; }
        public string Expires { get;  set; }
    }
}
