using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Security
{
    public class UserAccess
    {
        public int? Create { get; set; }
        public int? Read { get; set; }
        public int? Update { get; set; }
        public int? Delete { get; set; }

        public static UserAccess GetFullAccess()
        {
            return new UserAccess()
            {
                Create = 1,
                Read = 1,
                Update = 1,
                Delete = 1,
            };
        }
    }
}
