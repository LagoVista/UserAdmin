using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Security
{
    public class UserAccess
    {
        public const int Revoke = -1;
        public const int Inherit = 0;
        public const int Grant = 1;

        public int? Create { get; set; }
        public int? Read { get; set; }
        public int? Update { get; set; }
        public int? Delete { get; set; }

        public bool Any()
        {
            return (Create.HasValue && Create.Value != UserAccess.Revoke) ||
                   (Read.HasValue && Read.Value != UserAccess.Revoke) ||
                   (Update.HasValue && Delete.Value != UserAccess.Revoke) ||
                   (Update.HasValue && Delete.Value != UserAccess.Revoke);
        }

        public static UserAccess GetFullAccess()
        {
            return new UserAccess()
            {
                Create = UserAccess.Grant,
                Read = UserAccess.Grant,
                Update = UserAccess.Grant,
                Delete = UserAccess.Grant,
            };
        }

        public static UserAccess None()
        {
            return new UserAccess()
            {
                Create = UserAccess.Revoke,
                Read = UserAccess.Revoke,
                Update = UserAccess.Revoke,
                Delete = UserAccess.Revoke,
            };
        }
    }
}
