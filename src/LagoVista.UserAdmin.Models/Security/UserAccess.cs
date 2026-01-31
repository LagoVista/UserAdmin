// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 421edffaf65a5704a9356226c5c507e0d2b987492d50ee8a4efde894c12246b6
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(
        Domains.OrganizationDomain,
        UserAdminResources.Names.UserAccess_Name,
        UserAdminResources.Names.UserAccess_Help,
        UserAdminResources.Names.UserAccess_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class UserAccess
    {
        public const int Revoke = -1;
        public const int NotSpecified = 0;
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

        public static UserAccess GetNotSpecified()
        {
            return new UserAccess()
            {
                Create = UserAccess.NotSpecified,
                Read = UserAccess.NotSpecified,
                Update = UserAccess.NotSpecified,
                Delete = UserAccess.NotSpecified,
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

        public void RevokeNotSet()
        {
            if (!Create.HasValue || Create.Value == UserAccess.NotSpecified)
                Create = UserAccess.Revoke;

            if (!Read.HasValue || Read.Value == UserAccess.NotSpecified)
                Read = UserAccess.Revoke;

            if (!Update.HasValue || Update.Value == UserAccess.NotSpecified)
                Update = UserAccess.Revoke;

            if (!Delete.HasValue || Delete.Value == UserAccess.NotSpecified)
                Delete = UserAccess.Revoke;
        }
    }
}
