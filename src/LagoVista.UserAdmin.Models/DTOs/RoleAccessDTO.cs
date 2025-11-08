// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 644566c8e53e254ae70665d51fbe5d0c5dcad02c90a6e5e7d52bc89cbf277d22
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.DTOs
{
    public class RoleAccessDTO : TableStorageEntity
    {
        public string RoleId { get; set; }
        public string RoleKey { get; set; }
        public string RoleName { get; set; }
        public bool IsPublic { get; set; }

        public string OrganizationId { get; set; }
        public string OrganizationName { get; set; }

        public string CreatedById { get; set; }
        public string CreatedByName { get; set; }

        public string CreationDate { get; set; }

        public string ModuleId { get; set; }
        public string ModuleKey { get; set; }
        public string ModuleName { get; set; }

        public string AreaId { get; set; }
        public string AreaKey { get; set; }
        public string AreaName { get; set; }

        public string PageId { get; set; }
        public string PageKey { get; set; }
        public string PageName { get; set; }

        public string FeatureId { get; set; }
        public string FeatureKey { get; set; }
        public string FeatureName { get; set; }

        public string FunctionMapId { get; set; }
        public string FunctionMapKey { get; set; }
        public string FunctionMapName { get; set; }

        public string FunctionMapFunctionId { get; set; }
        public string FunctionMapFunctionKey { get; set; }
        public string FunctionMapFunctionName { get; set; }


        public int Create { get; set; }
        public int Read { get; set; }
        public int Update { get; set; }
        public int Delete { get; set; }
        public string Other { get; set; }

        public RoleAccess ToRoleAccess()
        {
            var roleAccess = new RoleAccess()
            {
                Id = RowKey,

                Role = EntityHeader.Create(RoleId, RoleKey, RoleName),
                Organization = EntityHeader.Create(OrganizationId, OrganizationName),
                CreatedBy = EntityHeader.Create(CreatedById, CreatedByName),

                Module = EntityHeader.Create(ModuleId, ModuleKey, ModuleName),

                CreationDate = CreationDate,

                Create = Create,
                Read = Read,
                Update = Update,
                Delete = Delete,
                Other = Other
            };

            if (!String.IsNullOrEmpty(AreaId) && !String.IsNullOrEmpty(AreaKey) && !String.IsNullOrEmpty(AreaName))
            {
                roleAccess.Area = EntityHeader.Create(AreaId, AreaKey, AreaName);
            }

            if (!String.IsNullOrEmpty(PageId) && !String.IsNullOrEmpty(PageKey) && !String.IsNullOrEmpty(PageName))
            {
                roleAccess.Page = EntityHeader.Create(PageId, PageKey, PageName);
            }

            if (!String.IsNullOrEmpty(FeatureId) && !String.IsNullOrEmpty(FeatureKey) && !String.IsNullOrEmpty(FeatureName))
            {
                roleAccess.Area = EntityHeader.Create(FeatureId, FeatureKey, FeatureName);
            }


            return roleAccess;
        }
    }
}
