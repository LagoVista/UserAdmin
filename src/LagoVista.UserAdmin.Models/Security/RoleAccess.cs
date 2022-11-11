﻿using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.DTOs;

namespace LagoVista.UserAdmin.Models.Security
{
    public class RoleAccess
    {
        public string Id { get; set; }
        public bool IsPublic { get; set; }
        public EntityHeader Role { get; set; }
        public EntityHeader Organization { get; set; }
        public string CreationDate { get; set; }
        public EntityHeader CreatedBy { get; set; }

        public EntityHeader Module { get; set; }
        public EntityHeader Area { get; set; }
        public EntityHeader Page { get; set; }
        public EntityHeader Feature { get; set; }

        public int Create { get; set; }
        public int Read { get; set; }
        public int Update { get; set; }
        public int Delete { get; set; }
        public string Other { get; set; }


        public RoleAccessDTO ToDTO()
        {
            return new RoleAccessDTO()
            {
                RowKey = Id,
                PartitionKey = Organization.Id,

                RoleId = Role.Id,
                RoleKey = Role.Key,
                RoleName = Role.Text,

                OrganizationId = Organization.Id,
                OrganizationName = Organization.Text,

                ModuleId = Module?.Id,
                ModuleKey = Module?.Key,
                ModuleName = Module?.Text,

                AreaId = Area?.Id,
                AreaKey = Area?.Key,
                AreaName  = Area?.Text,

                PageId = Page?.Id,
                PageKey = Page?.Key,
                PageName = Page?.Text,

                FeatureId = Feature?.Id,
                FeatureKey = Feature?.Key,
                FeatureName = Feature?.Text,
                
                IsPublic = IsPublic,

                CreatedById = CreatedBy.Id,
                CreatedByName = CreatedBy.Text,

                Create = Create,
                Read = Read,
                Update = Update,
                Delete = Delete,
                Other = Other
            };
        }
    }
}
