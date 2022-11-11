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
        public bool IsPublic { get; set; }

        public string OrganizationId { get; set; }
        public string OrganizationName { get; set; }

        public string CreatedById { get; set; }
        public string CreatedByName { get; set; }

        public string CreationDate { get; set; }

        public string ModuleId { get; set; }
        public string ModuleName { get; set; }

        public string AreaId { get; set; }
        public string AreaName { get; set; }

        public string PageId { get; set; }
        public string PageName { get; set; }

        public string FeatureId { get; set; }
        public string FeatureName { get; set; }

        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
        public string Other { get; set; }

        public RoleAccess ToRoleAccess()
        {
            return new RoleAccess()
            {
                Id = RowKey,

                Role = EntityHeader.Create(RoleId, RoleKey),
                Organization = EntityHeader.Create(OrganizationId, OrganizationName),
                CreatedBy = EntityHeader.Create(CreatedById, CreatedByName),

                Module = EntityHeader.Create(ModuleId, ModuleName),
                Area = EntityHeader.Create(AreaId, AreaName),
                Page = EntityHeader.Create(PageId, PageName),
                Feature = EntityHeader.Create(FeatureId, FeatureName),

                CreationDate = CreationDate,

                Create = Create,
                Read = Read,
                Update = Update,
                Delete = Delete,
                Other = Other
            };
        }
    }
}
